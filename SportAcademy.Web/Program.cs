using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(
        new SoftDeleteInterceptor(),
        new AuditingInterceptor()
    );
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserContextService, UserContextService>();

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
        Title = "My API",
        Version = "v1",
        Description = "Sample API with Swagger + OpenAPI"
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

app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notification");

app.MapControllers();

app.Run();
