using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Mappings.TraineeProfile;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.DBContext;
using SportAcademy.Application.Interfaces;
using SportAcademy.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateTraineeCommand).Assembly));

builder.Services.AddAutoMapper(typeof(TraineeProfile).Assembly);

builder.Services.AddScoped<ITraineeService, TraineeService>();

builder.Services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));

builder.Services.AddScoped<ITraineeRepository, TraineeRepository>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
