using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SportAcademy.Application;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure;
using SportAcademy.Infrastructure.Implementations;
using SportAcademy.Infrastructure.Localization;
using SportAcademy.Infrastructure.Options;
using SportAcademy.Infrastructure.Notifications;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Interceptors;
using SportAcademy.Infrastructure.Seeders;
using SportAcademy.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using SportAcademy.Web.Authorization;
using SportAcademy.Web.Filters;
using SportAcademy.Web.Middleware;
using SportAcademy.Web.Services;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, cfg) =>
    cfg.ReadFrom.Configuration(context.Configuration));

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;

    // Identity's default AllowedUserNameCharacters is an ASCII-only allow-list, which would
    // reject Arabic usernames even though the validator now permits them. Empty disables the
    // character check and defers entirely to CreateUserValidator/UpdateUserValidator.
    options.User.AllowedUserNameCharacters = string.Empty;
})
.AddErrorDescriber<LocalizedIdentityErrorDescriber>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserContextService, UserContextService>();

builder.Services.AddScoped<ITenantIdProvider, TenantIdProvider>();
builder.Services.AddScoped<IBranchAccessProvider, BranchAccessProvider>();
builder.Services.AddScoped<ICurrentLanguageProvider, CurrentLanguageProvider>();
builder.Services.AddScoped<ITenantSettingsLanguageReader, TenantSettingsLanguageReader>();
builder.Services.AddScoped<ITenantSettingsCurrencyReader, TenantSettingsCurrencyReader>();
builder.Services.AddScoped<ITenantClock, TenantClock>();
builder.Services.AddScoped<ILocalizationService, JsonLocalizationService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));

builder.Services.Configure<AppUrlSettings>(
    builder.Configuration.GetSection("AppSettings"));

builder.Services.Configure<TenantArchivalSettings>(
    builder.Configuration.GetSection("TenantArchival"));

builder.Services.AddScoped<AuditingInterceptor>();

builder.Services.AddScoped<SoftDeleteInterceptor>();

builder.Services.AddScoped<TenantSaveChangesInterceptor>();

builder.Services.AddScoped<AuditImmutabilityInterceptor>();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    var auditingInterceptor = sp.GetRequiredService<AuditingInterceptor>();
    var softDeleteInterceptor = sp.GetRequiredService<SoftDeleteInterceptor>();
    var tenantSaveChangesInterceptor = sp.GetRequiredService<TenantSaveChangesInterceptor>();
    var auditImmutabilityInterceptor = sp.GetRequiredService<AuditImmutabilityInterceptor>();
    options.AddInterceptors(
        auditingInterceptor, softDeleteInterceptor, tenantSaveChangesInterceptor, auditImmutabilityInterceptor);
});

var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/notification"))
            {
                context.Token = accessToken;
                return Task.CompletedTask;
            }
            if (context.Request.Cookies.ContainsKey("jwt"))
            {
                context.Token = context.Request.Cookies["jwt"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
// Records a Denied audit event for any authenticated user refused a /api/platform/* route -
// see PlatformDenialAuditResultHandler for why this can't be done from PermissionAuthorizationHandler.
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, PlatformDenialAuditResultHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddPolicy("per-user", httpContext =>
    {
        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        return RateLimitPartition.GetTokenBucketLimiter(userId, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 100,
            AutoReplenishment = true,
            QueueLimit = 20,
        });
    });

    options.AddPolicy("per-tenant", httpContext =>
    {
        var tenantId = httpContext.User.FindFirst("tenant_id")?.Value ?? "none";
        return RateLimitPartition.GetTokenBucketLimiter(tenantId, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 1000,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 1000,
            AutoReplenishment = true,
            QueueLimit = 50,
        });
    });

    options.AddPolicy("public", httpContext =>
    {
        // Partitioned per-IP (like "token-revoke" below) so one noisy/abusive client can't
        // exhaust a shared bucket and lock every other client out of login/refresh.
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
        });
    });

    options.AddPolicy("token-revoke", httpContext =>
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
        });
    });
});

// Cors:AllowedOrigins is read from configuration (appsettings.{Environment}.json or the
// CORS__AllowedOrigins__0, CORS__AllowedOrigins__1, ... environment variables) so production
// deployments can declare their real frontend origin(s) without editing code. Falls back to
// the local dev ports when the setting is absent.
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?.Where(o => !o.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase))
    .ToArray();

if (builder.Environment.IsProduction() && configuredOrigins is not { Length: > 0 })
{
    // Fail fast instead of silently falling back to the localhost dev origins below, which
    // would leave the real production frontend unable to call the API (or worse, mask a
    // misconfiguration as a mysterious CORS error at runtime).
    throw new InvalidOperationException(
        "Cors:AllowedOrigins is not configured for Production. Set it via the " +
        "CORS__AllowedOrigins__0 (and __1, __2, ...) environment variable(s) to the real " +
        "frontend origin(s) before starting the app.");
}

var allowedOrigins = configuredOrigins is { Length: > 0 }
    ? configuredOrigins
    :
    [
        "https://localhost:8080",
        "http://localhost:8080",
        "https://localhost:8081",
        "http://localhost:8081",
        "https://localhost:44306"
    ];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add Application layer services (MediatR, AutoMapper, Validators, Application Services)
builder.Services.AddApplicationServices();

// Add Infrastructure layer services (Repositories, External Clients, JWT)
builder.Services.AddInfrastructureServices();

// Seeding stays opt-in outside Development via Seeding:Enabled (Seeding__Enabled env var) so
// a real production deploy never gets demo data unless someone explicitly asks for it - e.g.
// a local IIS test box that needs a login-capable account and wants the demo dataset to test
// against. Computed here, ahead of the migration/seed call site below, purely so it's available
// in one place before builder.Build() - the file-logging email fallback below no longer depends
// on this at all, it now applies in every environment.
var seedingEnabled = builder.Environment.IsDevelopment()
    || builder.Configuration.GetValue<bool>("Seeding:Enabled");

// Register external HTTP client services (web layer specific). Both providers are registered so
// Email:Provider can choose between them at startup - switching is then a config change plus a
// restart, not a redeploy, and the one that isn't selected costs nothing but a typed HttpClient.
builder.Services.AddHttpClient<ResendEmailService>();
builder.Services.AddHttpClient<SendGridEmailService>();

// Read through the fully-layered configuration, so this reflects whatever actually won -
// appsettings.Development.json overrides the base file, and an Email__Provider environment
// variable overrides both. Which one was chosen is logged after the host is built (Serilog isn't
// configured until then), because the selection is otherwise invisible until the first send and
// a stale override in one layer looks exactly like the switch not having worked.
var emailProvider = builder.Configuration["Email:Provider"];
var useSendGrid = string.Equals(emailProvider, "SendGrid", StringComparison.OrdinalIgnoreCase);

builder.Services.AddScoped<IEmailService>(sp =>
{
    IEmailService sender = useSendGrid
        ? sp.GetRequiredService<SendGridEmailService>()
        : sp.GetRequiredService<ResendEmailService>();

    // Every environment, including Production, additionally records every outgoing link to a
    // file before the send is attempted, so an invitation/reset link is still recoverable when
    // the provider is unreachable, out of credits, misconfigured, or simply down -
    // SendOwnerPasswordResetLinkCommandHandler and InvitationCreatedHandler both already lean on
    // this file existing when they swallow a send failure instead of reporting it. Storage:LogsPath
    // (set to /app/logs in Production, backed by the backend_logs Docker volume so it survives
    // redeploys) is where this lands in a real deployment; falls back to ContentRootPath for local
    // dev, where that setting isn't configured.
    var invitationLinksPath = Path.Combine(
        builder.Configuration["Storage:LogsPath"] ?? builder.Environment.ContentRootPath,
        "invitation-links.txt");
    return new FileLoggingEmailServiceDecorator(sender, invitationLinksPath);
});

builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ResultStatusFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.UseInlineDefinitionsForEnums();

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SportAcademy API",
        Version = "v1",
        Description = "Manage Sport Academy System",
        Contact = new OpenApiContact
        {
            Name = "Sport Academy Team",
            Email = "abdulrahmannalatfy@gmail.com"
        }
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Scheme = "bearer",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token as: **Bearer [your_token]**",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

//builder.Services.AddOpenApi();

// SignalR:RedisConnection is only set once this app runs on more than one instance behind a
// load balancer - a single connection's SignalR groups/users otherwise live in that instance's
// memory alone, so a client connected to instance A never receives a push originating from
// instance B. Absent that setting (every environment today), SignalR falls back to its default
// in-memory backplane, which is correct and sufficient for a single instance.
var redisConnectionString = builder.Configuration["SignalR:RedisConnection"];
var signalRBuilder = builder.Services.AddSignalR();
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    signalRBuilder.AddStackExchangeRedis(redisConnectionString, options =>
    {
        options.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("SportAcademy");
    });
}

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

app.Logger.LogInformation(
    "Email provider: {Provider} (Email:Provider resolved to {Configured}, From {FromEmail})",
    useSendGrid ? "SendGrid" : "Resend",
    emailProvider ?? "(not set)",
    builder.Configuration["Email:FromEmail"]);

// Configure the HTTP request pipeline.
// Migrations run in every environment (single-instance IIS deploys have no migration
// step of their own). seedingEnabled was computed above, before builder.Build().
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    // Roles, the Feature/subscription-plan/nationality-category catalogs, and the SuperAdmin
    // account are cross-tenant shared data that must exist in every environment - including a
    // brand-new Production database with demo seeding off. Only the demo "Salmiya Academy"
    // tenant and its business data stay behind the seedingEnabled gate below.
    var seeder = scope.ServiceProvider.GetRequiredService<AppDataSeeder>();
    await seeder.EnsureCoreDataAsync();

    if (seedingEnabled)
    {
        await seeder.SeedDemoDataAsync();
    }
}

// The backend container is never published directly to the host - only reachable from Caddy
// over the internal docker-compose network - so it's safe to trust forwarded headers from any
// hop that can reach it at all, rather than pinning specific proxy IPs that vary per deployment.
// Must run before UseHttpsRedirection()/UseAuthentication() so they see the original
// scheme/client IP that Caddy forwards, not the plain-HTTP hop between Caddy and Kestrel.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

// Gates a suspended/archived tenant's uploaded files before UseStaticFiles() below ever sees
// the request - see the middleware's own comment for why this doesn't need general auth.
app.UseMiddleware<TenantFileAccessGuardMiddleware>();

// Serves uploaded images back out of wwwroot/uploads (LocalFileStorageService's write side) -
// plain disk-backed UseStaticFiles(), not MapStaticAssets(), since that one only serves assets
// baked in at build time and would never see a file an upload wrote at runtime. No auth: an
// avatar/logo/photo URL is meant to be publicly viewable wherever the app renders it, the same
// as any other CDN-hosted image would be.
app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthentication();

// After authentication on purpose: the tenant's configured language is only knowable once the
// user is resolved. TenantResolutionMiddleware runs earlier and could only see the header.
app.UseMiddleware<CultureResolutionMiddleware>();

// Before UseAuthorization(): a suspended/archived tenant should get a distinct 403
// TENANT_SUSPENDED/TENANT_ARCHIVED, not a permission-shaped 403 from PermissionAuthorizationHandler.
app.UseMiddleware<TenantStatusGuardMiddleware>();

// Enforces impersonation sessions are read-only and revocable before their own JWT expiry -
// see ImpersonationGuardMiddleware. Runs after the tenant-status guard on purpose: if the
// impersonated tenant itself gets suspended mid-session, that guard's rejection should win.
app.UseMiddleware<ImpersonationGuardMiddleware>();

// Must run between authentication and authorization, not after: PermissionAuthorizationHandler
// (invoked by UseAuthorization() below) resolves permissions through a tenant-scoped DB query
// (see PermissionResolver), so ITenantIdProvider has to be populated before authorization runs
// or every tenant-scoped lookup silently sees no tenant and resolves to zero permissions -
// which only surfaced once the resolver stopped being a pure JWT-claim check. This block used
// to run after UseAuthorization() (harmless back when the permission handler only inspected
// the token's claims directly), and was left there when the resolver changed.
app.Use(async (context, next) =>
{
    var userContext = context.RequestServices.GetRequiredService<IUserContextService>();

    if (userContext.IsAuthenticated && userContext.TenantId == null)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Tenant identifier is missing from the authentication token."
        });
        return;
    }

    var tenantIdProvider = context.RequestServices.GetRequiredService<ITenantIdProvider>();
    tenantIdProvider.SetTenantId(userContext.TenantId);

    // Only "Employee" is branch-restricted (see IBranchAccessProvider) - every other
    // authenticated role stays unrestricted. Queried fresh per request (not baked into the
    // JWT) so a branch grant/revoke made via the Users & Roles page takes effect on the very
    // next request, not just after the access token is refreshed.
    if (userContext.IsAuthenticated && userContext.Role.Contains("Employee") && userContext.UserId is { } currentUserId)
    {
        var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();
        var allowedBranchIds = await db.UserBranchAccesses
            .Where(a => a.UserId == currentUserId)
            .Select(a => a.BranchId)
            .ToListAsync();

        // An Employee with zero UserBranchAccess rows hasn't been assigned any branches yet
        // (e.g. an Employee-role account that predates this feature) - treat that as
        // unrestricted rather than locking them out of every branch-scoped list. Restriction
        // only takes effect once an admin has actually chosen at least one branch for them,
        // via invite-time assignment (CreateInvitationCommandHandler requires this for new
        // Employee invites) or the "Manage branch access" action on the Users & Roles page.
        if (allowedBranchIds.Count > 0)
        {
            var branchAccessProvider = context.RequestServices.GetRequiredService<IBranchAccessProvider>();
            branchAccessProvider.SetBranchAccess(true, allowedBranchIds);
        }
    }

    await next();
});

app.UseAuthorization();

app.UseRateLimiter();

app.MapHub<NotificationHub>("/hubs/notification");

app.MapControllers();

app.MapHealthChecks("/health");

try
{
    Log.Information("Starting SportAcademy API");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SportAcademy API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

