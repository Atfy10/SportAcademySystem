using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SportAcademy.Application.Behaviors;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.TraineeProfile;
using SportAcademy.Application.Services;
using SportAcademy.Application.Validators.TraineeValidators;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Services;

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
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FeatureGateBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(BranchAccessValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PaginationNormalizationBehavior<,>));
            // Registered last so it wraps closest to the handler - every other behavior above
            // has already run (and could still short-circuit) before a transaction opens here.
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PlatformAuditBehavior<,>));

            // Register FluentValidation validators
            services.AddValidatorsFromAssembly(typeof(CreateTraineeValidator).Assembly);

            // Register AutoMapper profiles
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(TraineeProfile).Assembly);
            });

            // Register Application Services
            services.AddScoped<SubDetailsManagementService>();
            services.AddScoped<IFinanceLedgerService, FinanceLedgerService>();
            services.AddScoped<ISubscriptionCreationService, SubscriptionCreationService>();
            services.AddScoped<TraineeGroupService>();

            return services;
        }
    }
}
