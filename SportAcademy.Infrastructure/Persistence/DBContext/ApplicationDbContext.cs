using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;
using SportAcademy.Infrastructure.Persistence.Converters;
using SportAcademy.Infrastructure.Persistence.Views.AdminViews;
using SportAcademy.Infrastructure.Persistence.Views.CoachViews;
using SportAcademy.Infrastructure.Persistence.Views.EmployeeViews;
using SportAcademy.Infrastructure.Persistence.Views.GroupViews;
using SportAcademy.Infrastructure.Persistence.Views.ScheduleViews;
using SportAcademy.Infrastructure.Persistence.Views.Interfaces;
using SportAcademy.Infrastructure.Persistence.Views.TraineeViews;
using System.Linq.Expressions;
using System.Reflection;

namespace SportAcademy.Infrastructure.Persistence.DBContext
{
    public class ApplicationDbContext
        : IdentityDbContext<AppUser, AppRole, Guid, IdentityUserClaim<Guid>, AppUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly IBranchAccessProvider _branchAccessProvider;
        public Guid? CurrentTenantId => _tenantIdProvider.TenantId;

        // Read by the IBranchScoped query filter below (and the explicit navigated filters for
        // Enrollment/Attendance/SessionOccurrence/Coach/ExcuseRequest) - false/empty for every
        // role except "Employee", see IBranchAccessProvider.
        public bool IsBranchRestricted => _branchAccessProvider.IsRestricted;
        public IReadOnlyList<int> CurrentAllowedBranchIds => _branchAccessProvider.AllowedBranchIds;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantIdProvider tenantIdProvider,
            IBranchAccessProvider branchAccessProvider)
            : base(options)
        {
            _tenantIdProvider = tenantIdProvider;
            _branchAccessProvider = branchAccessProvider;
        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Branch> Branchs { get; set; }
        public DbSet<Domain.Entities.Translations.BranchTranslation> BranchTranslations { get; set; } = null!;
        public DbSet<Coach> Coachs { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<ExcuseRequest> ExcuseRequests { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<Domain.Entities.Translations.PaymentTypeTranslation> PaymentTypeTranslations { get; set; } = null!;
        public DbSet<TraineeGroup> TraineeGroups { get; set; }
        public DbSet<Domain.Entities.Translations.TraineeGroupTranslation> TraineeGroupTranslations { get; set; } = null!;
        public DbSet<GroupSchedule> GroupSchedules { get; set; }
        public DbSet<SessionOccurrence> SessionOccurrences { get; set; }
        public DbSet<Sport> Sports { get; set; }
        public DbSet<Domain.Entities.Translations.SportTranslation> SportTranslations { get; set; } = null!;
        public DbSet<SportBranch> SportBranchs { get; set; }
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
        public DbSet<TraineeCareerEvent> TraineeCareerEvents { get; set; }
        public DbSet<Family> Families { get; set; }
        public DbSet<Domain.Entities.Translations.FamilyTranslation> FamilyTranslations { get; set; } = null!;
        public DbSet<NationalityCategory> NationalityCategories { get; set; }
        public DbSet<Domain.Entities.Translations.NationalityCategoryTranslation> NationalityCategoryTranslations { get; set; } = null!;
        public DbSet<TraineeMedicalCondition> TraineeMedicalConditions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserPermissionOverride> UserPermissionOverrides { get; set; }
        public DbSet<UserBranchAccess> UserBranchAccesses { get; set; }
        public DbSet<CoachBranchAccess> CoachBranchAccesses { get; set; }
        public DbSet<Domain.Entities.Finance.Invoice> Invoices { get; set; }
        public DbSet<Domain.Entities.Finance.InvoiceLine> InvoiceLines { get; set; }
        public DbSet<Domain.Entities.Finance.PaymentAllocation> PaymentAllocations { get; set; }
        public DbSet<Domain.Entities.Finance.ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<Domain.Entities.Finance.Expense> Expenses { get; set; }
        public DbSet<Domain.Entities.Finance.SalaryPayment> SalaryPayments { get; set; }
        public DbSet<Domain.Entities.Finance.DiscountCode> DiscountCodes { get; set; }
        public DbSet<Domain.Entities.Finance.SubscriptionDiscountRequest> SubscriptionDiscountRequests { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantFeature> TenantFeatures { get; set; }
        public DbSet<TenantProfile> TenantProfiles { get; set; }
        public DbSet<TenantSettings> TenantSettings { get; set; }
        public DbSet<TenantSubscription> TenantSubscriptions { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures { get; set; }
        public DbSet<Domain.Entities.Marketing.Lead> Leads { get; set; }

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

        // Forces Kind=Utc on every DateTime/DateTime? read from the database (SQL Server's
        // datetime2 has no offset, so EF would otherwise materialize Kind=Unspecified regardless
        // of what wrote it) - see UtcDateTimeConverter for the full reasoning. DateTimeOffset
        // columns (just ASP.NET Identity's own AppUser.LockoutEnd) are a different CLR type and
        // untouched by this.
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
            configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
        }

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

            // Entities with no BranchId column of their own still need branch-scoping - each
            // path below is the shortest required (non-nullable) navigation chain to a
            // BranchId-bearing entity. Kept as an explicit map (not reflection) since the path
            // differs per entity and can't be derived generically the way IBranchScoped's
            // direct property can.
            // Coach is deliberately NOT branch-scoped here (it was, and that was a bug - see
            // below). A coach's *employment* branch (Employee.BranchId) and the branch of a
            // group they *teach* are independent - coaches can and do teach groups at a branch
            // other than the one that employs them. TraineeGroup/SessionOccurrence/etc. already
            // carry their own correct branch scope; every card/list projection for those entities
            // also joins TraineeGroup -> Coach (a required, non-nullable navigation) to show the
            // coach's name. If Coach were branch-scoped too, that join becomes an INNER JOIN
            // against an ADDITIONALLY filtered Coach row - a coach employed at a different branch
            // than the group they teach silently drops the entire group/session/etc. row from a
            // branch-restricted caller's results, even though the group's own branch is one
            // they're allowed to see. (Confirmed: branch-1 groups taught by a branch-2/3-employed
            // coach vanished from every list and page for a branch-1-restricted Employee, while
            // TotalCount - a plain COUNT with no Coach join - still reported the true total.)
            var branchNavigationPaths = new Dictionary<Type, string[]>
            {
                [typeof(Enrollment)] = ["TraineeGroup", "BranchId"],
                [typeof(Attendance)] = ["Enrollment", "TraineeGroup", "BranchId"],
                [typeof(SessionOccurrence)] = ["GroupSchedule", "TraineeGroup", "BranchId"],
                [typeof(ExcuseRequest)] = ["Enrollment", "TraineeGroup", "BranchId"],
            };

            // These three still implement IBranchScoped (their BranchId is real and meaningful)
            // but are excluded from the AUTOMATIC global filter below. Each represents a
            // "home"/"registered at" branch that can legitimately differ from the branch of an
            // unrelated, already-correctly-scoped entity that merely references it: a Trainee's
            // registered branch can differ from a group they're enrolled in at another branch, a
            // SubscriptionDetails' branch can differ from its Enrollment's group's branch, and an
            // Employee's employment branch can differ from a group their Coach teaches at. Making
            // any of these auto-filtered means every query that reaches them via a required
            // navigation from that unrelated root (Enrollment.Trainee, Enrollment.
            // SubscriptionDetails, TraineeGroup.Coach.Employee) silently drops the whole root row
            // whenever the referenced entity's own branch isn't in the caller's allowed set -
            // even though the root's own (correct) branch scope already passed. The
            // Trainees/Employees list endpoints - where one of these IS the query's actual
            // subject - apply the same branch check explicitly instead (see
            // BaseRepository.GetAllPaginatedAsync and each repository's hand-written list
            // methods).
            var branchAutoFilterExclusions = new HashSet<Type>
            {
                typeof(Trainee), typeof(SubscriptionDetails), typeof(Employee),
                // SportPrice: shares SubscriptionDetails' own BranchId by construction (its
                // composite key is Sport+Branch+SubscriptionType), so it inherits the exact same
                // mismatch whenever SubscriptionDetails.BranchId differs from the enrolled
                // group's branch. Payment/Invoice: a payment or invoice can be processed at a
                // branch other than the one the underlying subscription/enrollment belongs to
                // (e.g. central billing, or a trainee paying at whichever branch they're
                // visiting) - same reasoning as Trainee's registered branch vs. enrolled group.
                typeof(Domain.Entities.SportPrice), typeof(Domain.Entities.Payment), typeof(Domain.Entities.Finance.Invoice),
            };

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var isTenantScoped = typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType)
                                     && entityType.ClrType != typeof(Tenant);
                var isSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType);

                var parameter = Expression.Parameter(entityType.ClrType, "e");

                Expression? branchIdAccessor = typeof(IBranchScoped).IsAssignableFrom(entityType.ClrType)
                    && !branchAutoFilterExclusions.Contains(entityType.ClrType)
                    ? Expression.Property(parameter, "BranchId")
                    : branchNavigationPaths.TryGetValue(entityType.ClrType, out var path)
                        ? path.Aggregate((Expression)parameter, Expression.Property)
                        : null;
                var isBranchScoped = branchIdAccessor is not null;

                if (!isTenantScoped && !isSoftDeletable && !isBranchScoped)
                    continue;

                Expression? body = null;

                if (isTenantScoped)
                {
                    var dbContext = Expression.Constant(this);
                    var currentTenantId = Expression.Property(dbContext, nameof(CurrentTenantId));

                    body = Expression.Equal(
                        Expression.Convert(
                            Expression.Property(parameter, "TenantId"),
                            typeof(Guid?)),
                        currentTenantId);
                }

                if (isSoftDeletable)
                {
                    var notDeleted = Expression.Equal(
                        Expression.Property(parameter, "IsDeleted"),
                        Expression.Constant(false));

                    body = body != null
                        ? Expression.AndAlso(body, notDeleted)
                        : notDeleted;
                }

                if (isBranchScoped)
                {
                    // Unrestricted (every role except "Employee") always passes; a restricted
                    // user only sees rows whose branch is in their current allow-list - an
                    // Employee granted zero branches sees zero rows here, not everything.
                    var dbContext = Expression.Constant(this);
                    var isBranchRestricted = Expression.Property(dbContext, nameof(IsBranchRestricted));
                    var allowedBranchIds = Expression.Property(dbContext, nameof(CurrentAllowedBranchIds));
                    var containsCall = Expression.Call(
                        typeof(Enumerable), nameof(Enumerable.Contains), [typeof(int)],
                        allowedBranchIds, branchIdAccessor!);

                    var branchAllowed = Expression.OrElse(Expression.Not(isBranchRestricted), containsCall);

                    body = body != null
                        ? Expression.AndAlso(body, branchAllowed)
                        : branchAllowed;
                }

                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(Expression.Lambda(body!, parameter));
            }

            // Apply tenant query filter to all IModelView (SQL view) entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(IModelView).IsAssignableFrom(e.ClrType)))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");

                var viewBody = Expression.Equal(
                    Expression.Convert(
                        Expression.Property(parameter, "TenantId"),
                        typeof(Guid?)),
                    Expression.Property(Expression.Constant(this), nameof(CurrentTenantId)));

                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(Expression.Lambda(viewBody, parameter));
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
