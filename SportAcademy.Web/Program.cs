using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SportAcademy.Application.Behaviors;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.TraineeProfile;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.DBContext;
using SportAcademy.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
