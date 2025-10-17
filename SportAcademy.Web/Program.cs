using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SportAcademy.Application.Behaviors;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.TraineeProfile;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.DBContext;
using SportAcademy.Infrastructure.Implementations;
using SportAcademy.Infrastructure.Notifications;
using SportAcademy.Infrastructure.Repositories;
using SportAcademy.Web;
using System;

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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateTraineeCommand).Assembly));

//builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

//builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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

builder.Services.AddControllers();

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
    await DatabaseInitializer.SeedDatabase(scope.ServiceProvider);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notification");

app.MapControllers();

app.Run();
