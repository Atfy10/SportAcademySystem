using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SportAcademy.Application;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure;
using SportAcademy.Infrastructure.Implementations.OpenAi;
using SportAcademy.Infrastructure.Implementations.OpenRouter;
using SportAcademy.Infrastructure.Notifications;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Interceptors;
using SportAcademy.Infrastructure.Seeders;
using SportAcademy.Web.Services;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, cfg) =>
    cfg.ReadFrom.Configuration(context.Configuration));

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    // Example password settings (optional)
    options.Password.RequiredLength = 4;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserContextService, UserContextService>();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://localhost:8080",
            "http://localhost:8080",
            "https://localhost:8081",
            "http://localhost:8081",
            "https://localhost:44306"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add Application layer services (MediatR, AutoMapper, Validators, Application Services)
builder.Services.AddApplicationServices();

// Add Infrastructure layer services (Repositories, External Clients, JWT)
builder.Services.AddInfrastructureServices();

// Register external HTTP client services (web layer specific)
builder.Services.AddHttpClient<IOpenAiChatClient, OpenAiChatClient>();
builder.Services.AddHttpClient<IOpenRouterClient, OpenRouterClient>();

builder.Services.AddControllers()
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
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        //await dbContext.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<AppDataSeeder>();
        await seeder.SeedAsync();
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

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

