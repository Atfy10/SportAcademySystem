using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SportAcademy.Application.Behaviors;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.TraineeProfile;
using SportAcademy.Application.Services;
using SportAcademy.Application.Validators.TraineeValidators;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.Implementations;
using SportAcademy.Infrastructure.Notifications;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Interceptors;
using SportAcademy.Infrastructure.Persistence.Repositories;
using SportAcademy.Infrastructure.Seeders;
using SportAcademy.Web;
using SportAcademy.Web.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
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

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    var auditingInterceptor = sp.GetRequiredService<AuditingInterceptor>();
    var softDeleteInterceptor = sp.GetRequiredService<SoftDeleteInterceptor>();
    options.AddInterceptors(auditingInterceptor, softDeleteInterceptor);
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
            //var accessToken = context.Request.Query["access_token"];
            //var path = context.HttpContext.Request.Path;
            //if (!string.IsNullOrEmpty(accessToken) &&
            //    (path.StartsWithSegments("/hubs/notification")))
            //{
            //    context.Token = accessToken;
            //    return Task.CompletedTask;
            //}
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
        policy.WithOrigins("https://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateTraineeCommand).Assembly));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

builder.Services.AddValidatorsFromAssembly(typeof(CreateTraineeValidator).Assembly);

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddAutoMapper(typeof(TraineeProfile).Assembly);

builder.Services.AddScoped<ITraineeService, TraineeService>();

builder.Services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));

builder.Services.AddScoped<ITraineeRepository, TraineeRepository>();

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IBranchRepository, BranchRepository>();

builder.Services.AddScoped<ISportRepository, SportRepository>();

builder.Services.AddScoped<ISportBranchRepository, SportBranchRepository>();

builder.Services.AddScoped<ISportPriceRepository, SportPriceRepository>();

builder.Services.AddScoped<ISubscriptionTypeRepository, SubscriptionTypeRepository>();

builder.Services.AddScoped<ISportTraineeRepository, SportTraineeRepository>();

builder.Services.AddScoped<ISportBranchRepository, SportBranchRepository>();

builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();

builder.Services.AddScoped<ISessionOccurrenceRepository, SessionOccurrenceRepository>();

builder.Services.AddScoped<ITraineeGroupRepository, TraineeGroupRepository>();

builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

builder.Services.AddScoped<ISubscriptionDetailsRepository, SubscriptionDetailsRepository>();

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddScoped<SubDetailsManagementService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
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

builder.Services.AddUserSeeder();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await DatabaseInitializer.SeedDatabase(scope.ServiceProvider);

    await DatabaseSeeder.SeedDatabase(dbContext, logger);
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notification");

app.MapControllers();

app.Run();

