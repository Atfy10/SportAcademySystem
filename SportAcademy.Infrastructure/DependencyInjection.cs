using Microsoft.Extensions.DependencyInjection;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.Implementations;
using SportAcademy.Infrastructure.Persistence.Repositories;

namespace SportAcademy.Infrastructure
{
    /// <summary>
    /// Extension methods for registering Infrastructure layer services.
    /// This centralizes all repository and external service client registrations.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Register generic repository
            services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));

            // Register specific repositories
            services.AddScoped<ITraineeRepository, TraineeRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<ISportRepository, SportRepository>();
            services.AddScoped<ISportBranchRepository, SportBranchRepository>();
            services.AddScoped<ISportPriceRepository, SportPriceRepository>();
            services.AddScoped<ISubscriptionTypeRepository, SubscriptionTypeRepository>();
            services.AddScoped<ISportTraineeRepository, SportTraineeRepository>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<ISessionOccurrenceRepository, SessionOccurrenceRepository>();
            services.AddScoped<ITraineeGroupRepository, TraineeGroupRepository>();
            services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            services.AddScoped<ISubscriptionDetailsRepository, SubscriptionDetailsRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IFamilyRepository, FamilyRepository>();
            services.AddScoped<INationalityCategoryRepository, NationalityCategoryRepository>();
            services.AddScoped<ICoachRepository, CoachRepository>();
            services.AddScoped<IChatConversationRepository, ChatConversationRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IVideoAnalysisRepository, VideoAnalysisRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Register JWT token service
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            // Register Notification Service
            services.AddScoped<INotificationService, NotificationService>();

            // Register Domain Services
            services.AddScoped<ITraineeService, TraineeService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<ITraineeCodeGenerator, SqlTraineeCodeGenerator>();

            return services;
        }
    }
}
