using Microsoft.Extensions.DependencyInjection;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.BackgroundServices;
using SportAcademy.Infrastructure.Implementations;
using SportAcademy.Infrastructure.Persistence;
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
            services.AddScoped<IUserPermissionOverrideRepository, UserPermissionOverrideRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IFinancialDocumentNumberGenerator, SqlFinancialDocumentNumberGenerator>();
            services.AddScoped<IFamilyRepository, FamilyRepository>();
            services.AddScoped<INationalityCategoryRepository, NationalityCategoryRepository>();
            services.AddScoped<ICoachRepository, CoachRepository>();
            services.AddScoped<IChatConversationRepository, ChatConversationRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IVideoAnalysisRepository, VideoAnalysisRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Register JWT token service
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            // Register permission resolver (deny-capable, server-side authorization source of
            // truth - see PermissionResolver for the resolution order). Registered as both
            // interfaces against the same singleton-scoped-cache-backed instance so the
            // authorization handler and the write paths that must invalidate it share one cache.
            services.AddMemoryCache();
            services.AddScoped<PermissionResolver>();
            services.AddScoped<IPermissionResolver>(sp => sp.GetRequiredService<PermissionResolver>());
            services.AddScoped<IPermissionCacheInvalidator>(sp => sp.GetRequiredService<PermissionResolver>());

            // Register Notification Service
            services.AddScoped<INotificationService, NotificationService>();

            // Register Realtime Service
            services.AddScoped<IRealtimeService, RealtimeService>();

            // Register Domain Services
            services.AddScoped<ITraineeService, TraineeService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<ITraineeCodeGenerator, SqlTraineeCodeGenerator>();

            // Register background services
            services.AddHostedService<RefreshTokenCleanupService>();
            services.AddHostedService<InvitationExpiryService>();
            services.AddHostedService<TenantArchivalService>();
            services.AddHostedService<EmailQueueCleanupService>();

            // Register seeders
            services.AddScoped<Seeders.AppDataSeeder>();

            // Register Invitation Repository (new pattern)
            services.AddScoped<IInvitationRepository, InvitationRepository>();

            // Register Tenant Repository (new pattern)
            services.AddScoped<ITenantRepository, TenantRepository>();

            // Register Tenant Audit Repository
            services.AddScoped<ITenantAuditRepository, TenantAuditRepository>();

            // Register Invitation Token Service
            services.AddScoped<IInvitationTokenService, InvitationTokenService>();

            // Register Unit of Work (new pattern)
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // IEmailService is registered in Program.cs via AddHttpClient<IEmailService,
            // SendGridEmailService>() - it needs an injected HttpClient, matching the pattern
            // used for IOpenAiChatClient/IOpenRouterClient.

            // Register Application URL Provider
            services.AddScoped<IAppUrlProvider, AppUrlProvider>();

            return services;
        }
    }
}
