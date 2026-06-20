using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SportAcademy.Application.Behaviors;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Application.Validators.TraineeValidators;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application
{
    /// <summary>
    /// Extension methods for registering Application layer services.
    /// This centralizes all MediatR, AutoMapper, Validation, and Application Services configuration.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register MediatR with request/response handlers from the Application assembly
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CreateTraineeCommand).Assembly);
            });

            // Register pipeline behaviors (cross-cutting concerns)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PaginationNormalizationBehavior<,>));

            // Register FluentValidation validators
            services.AddValidatorsFromAssembly(typeof(CreateTraineeValidator).Assembly);

            // Register Application Services
            services.AddScoped<IChatBotService, ChatBotService>();
            services.AddScoped<SubDetailsManagementService>();
            services.AddScoped<TraineeGroupService>();

            return services;
        }
    }
}
