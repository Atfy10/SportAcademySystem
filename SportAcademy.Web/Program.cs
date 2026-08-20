using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SportAcademy.Application;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure;
using SportAcademy.Infrastructure.Implementations;
using SportAcademy.Infrastructure.Implementations.OpenAi;
using SportAcademy.Infrastructure.Implementations.OpenRouter;
using SportAcademy.Infrastructure.Options;
using SportAcademy.Infrastructure.Notifications;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Interceptors;
using SportAcademy.Infrastructure.Seeders;
using SportAcademy.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
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
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserContextService, UserContextService>();

builder.Services.AddScoped<ITenantIdProvider, TenantIdProvider>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));

builder.Services.Configure<AppUrlSettings>(
    builder.Configuration.GetSection("AppSettings"));

builder.Services.Configure<TenantArchivalSettings>(
    builder.Configuration.GetSection("TenantArchival"));

builder.Services.AddScoped<AuditingInterceptor>();

builder.Services.AddScoped<SoftDeleteInterceptor>();

builder.Services.AddScoped<TenantSaveChangesInterceptor>();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    var auditingInterceptor = sp.GetRequiredService<AuditingInterceptor>();
    var softDeleteInterceptor = sp.GetRequiredService<SoftDeleteInterceptor>();
    var tenantSaveChangesInterceptor = sp.GetRequiredService<TenantSaveChangesInterceptor>();
    options.AddInterceptors(auditingInterceptor, softDeleteInterceptor, tenantSaveChangesInterceptor);
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
// against. Computed here (not just at the migration/seed call site below) because it also
// decides whether the file-logging email fallback applies - a box seeding demo data is a
// local test box, not a real deployment, and shouldn't need a real SendGrid key just to read
// an invitation link.
var seedingEnabled = builder.Environment.IsDevelopment()
    || builder.Configuration.GetValue<bool>("Seeding:Enabled");

// Register external HTTP client services (web layer specific)
builder.Services.AddHttpClient<IOpenAiChatClient, OpenAiChatClient>();
builder.Services.AddHttpClient<IOpenRouterClient, OpenRouterClient>();
builder.Services.AddHttpClient<SendGridEmailService>();
if (seedingEnabled)
{
    var devInvitationLinksPath = Path.Combine(builder.Environment.ContentRootPath, "dev-invitation-links.txt");
    builder.Services.AddScoped<IEmailService>(sp =>
        new FileLoggingEmailServiceDecorator(sp.GetRequiredService<SendGridEmailService>(), devInvitationLinksPath));
}
else
{
    builder.Services.AddScoped<IEmailService>(sp => sp.GetRequiredService<SendGridEmailService>());
}

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

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Migrations run in every environment (single-instance IIS deploys have no migration
// step of their own). seedingEnabled was computed above, before builder.Build().
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    if (seedingEnabled)
    {
        var seeder = scope.ServiceProvider.GetRequiredService<AppDataSeeder>();
        await seeder.SeedAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

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
    await next();
});

app.MapHub<NotificationHub>("/hubs/notification");

app.MapControllers();

if (app.Environment.IsProduction())
{
    app.MapGet("/health", () => Results.Ok("API Running"));
}

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

