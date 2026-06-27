using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;
using SportAcademy.Infrastructure.Persistence.Views.AdminViews;
using SportAcademy.Infrastructure.Persistence.Views.CoachViews;
using SportAcademy.Infrastructure.Persistence.Views.EmployeeViews;
using SportAcademy.Infrastructure.Persistence.Views.GroupViews;
using SportAcademy.Infrastructure.Persistence.Views.ScheduleViews;
using SportAcademy.Infrastructure.Persistence.Views.TraineeViews;
using System.Linq.Expressions;
using System.Reflection;

namespace SportAcademy.Infrastructure.Persistence.DBContext
{
    public class ApplicationDbContext
        : IdentityDbContext<AppUser, AppRole, Guid, IdentityUserClaim<Guid>, AppUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Branch> Branchs { get; set; }
        public DbSet<Coach> Coachs { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<TraineeGroup> TraineeGroups { get; set; }
        public DbSet<GroupSchedule> GroupSchedules { get; set; }
        public DbSet<SessionOccurrence> SessionOccurrences { get; set; }
        public DbSet<Sport> Sports { get; set; }
        public DbSet<SportBranch> SportBranchs { get; set; }
        public DbSet<ChatConversation> ChatConversations { get; set; }
        public DbSet<OpenAiMessage> ChatMessages { get; set; }
        public DbSet<SportPrice> SportPrices { get; set; }
        public DbSet<SportSubscriptionType> SportSubscriptionTypes { get; set; }
        public DbSet<SportTrainee> SportTrainees { get; set; }
        public DbSet<SubscriptionDetails> SubscriptionDetails { get; set; }
        public DbSet<SubscriptionType> SubscriptionTypes { get; set; }
        public DbSet<Trainee> Trainees { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }
        public DbSet<NotificationGroupMember> NotificationGroupMembers { get; set; }
        public DbSet<TraineeCodesHistory> TraineeCodesHistory { get; set; }
        public DbSet<Family> Families { get; set; }
        public DbSet<NationalityCategory> NationalityCategories { get; set; }
        public DbSet<VideoAnalysis> VideoAnalyses { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantFeature> TenantFeatures { get; set; }
        public DbSet<TenantProfile> TenantProfiles { get; set; }
        public DbSet<TenantSettings> TenantSettings { get; set; }
        public DbSet<TenantSubscription> TenantSubscriptions { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures { get; set; }

        //  View for reporting purposes
        public DbSet<AdminBasicViews> AdminBasicViews { get; set; }
        public DbSet<CoachScheduleView> CoachScheduleViews { get; set; }
        public DbSet<CoachSkillView> CoachSkillViews { get; set; }
        public DbSet<EmployeeBasicView> EmployeeBasicViews { get; set; }
        public DbSet<EmployeeWorkView> EmployeeWorkViews { get; set; }
        public DbSet<GroupBasicView> GroupBasicViews { get; set; }
        public DbSet<ScheduleDailyView> ScheduleDailyViews { get; set; }
        public DbSet<ScheduleWeeklyView> ScheduleWeeklyViews { get; set; }
        public DbSet<TraineeBasicView> TraineeBasicViews { get; set; }
        public DbSet<TraineeAttendanceView> TraineeAttendanceViews { get; set; }
        public DbSet<TraineeSessionView> TraineeSessionViews { get; set; }
        public DbSet<TraineeScheduleView> TraineeScheduleViews { get; set; }
        public DbSet<TraineeSubscriptionView> TraineeSubscriptionViews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(ITenantScoped).IsAssignableFrom(e.ClrType) && e.ClrType != typeof(Tenant)))
            {
                var tenantFk = entityType.GetForeignKeys()
                    .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Tenant)
                        && fk.Properties.Any(p => p.Name == "TenantId"));

                if (tenantFk?.PrincipalToDependent != null)
                    continue;

                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(Tenant), "Tenant")
                    .WithMany()
                    .HasForeignKey("TenantId")
                    .OnDelete(DeleteBehavior.Restrict);
            }

            modelBuilder.HasSequence<int>("FamilyCodeSequence")
                .StartsAt(1)
                .IncrementsBy(1)
                .HasMin(1)
                .HasMax(int.MaxValue)
                .IsCyclic(false);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<DateTime>("CreatedAt")
                        .IsRequired();

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<string?>("CreatedBy")
                        .IsRequired(false);

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<DateTime?>("UpdatedAt")
                        .IsRequired(false);

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<string?>("UpdatedBy")
                        .IsRequired(false);
                }

                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");

                    var filter = Expression.Lambda(
                        Expression.Equal(
                            Expression.Property(
                                parameter,
                                "IsDeleted"
                            ),
                            Expression.Constant(false)
                        ),
                        parameter
                    );

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<bool>("IsDeleted")
                        .HasDefaultValue(false)
                        .IsRequired();

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<DateTime?>("DeletedAt")
                        .IsRequired(false);

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<string?>("DeletedBy")
                        .IsRequired(false);

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .HasQueryFilter(filter);
                }

                if (typeof(Person).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50);

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50);

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<string>("SSN")
                        .IsRequired()
                        .HasMaxLength(14);

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<DateOnly>("BirthDate")
                        .IsRequired();

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<Gender>("Gender")
                        .HasConversion<string>()
                        .IsRequired();

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<Nationality>("Nationality")
                        .HasConversion<string>()
                        .IsRequired();

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(12);

                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property<string?>("SecondPhoneNumber")
                        .HasMaxLength(12);
                }
            }

            base.OnModelCreating(modelBuilder);

            // Identity base configures AppUserRole relationships without navigation names,
            // causing EF Core to discover AppUser/AppRole.UserRoles by convention and create
            // duplicate shadow FKs (RoleId1, UserId1). Re-configure with proper navigations.
            modelBuilder.Entity<AppUserRole>(ur =>
            {
                ur.HasOne(x => x.User)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.UserId);

                ur.HasOne(x => x.Role)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.RoleId);
            });
        }
    }
}
