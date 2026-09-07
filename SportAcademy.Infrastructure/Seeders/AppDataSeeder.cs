using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SportAcademy.Domain.Authorization;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;
using SportAcademy.Domain.ValueObjects;
using SportAcademy.Infrastructure.Persistence.DBContext;
using System.Security.Claims;

namespace SportAcademy.Infrastructure.Seeders
{
    public class AppDataSeeder
    {
        /// <summary>
        /// What a private group's place costs relative to the same plan's public price. Demo
        /// figure only - real academies set their own in the pricing matrix.
        /// </summary>
        private const decimal PrivatePriceMultiplier = 2.2m;

        // Used only for the demo Owner account below - the real SuperAdmin's password comes from
        // required config, never a hardcoded literal (see EnsureSystemTenantAndSuperAdminAsync).
        private const string DefaultPassword = "Admin@123";

        private const string SuperAdminUserName = "abdulrahman";
        private const string SuperAdminEmail = "abdulrahmanalatfy@auraacademys.com";
        private const string SuperAdminPhoneNumber = "+201096042061";

        private static readonly string[] KuwaitiAreas =
        [
            "Salmiya", "Hawally", "Jabriya", "Fintas", "Mahboula",
            "Mangaf", "Fahaheel", "Ahmadi", "Farwaniya", "Jleeb Al-Shuyoukh",
            "Sabah Al-Salem", "Rumaithiya", "Bayan", "Mishref", "Salwa",
            "Abdullah Al-Mubarak", "Jaber Al-Ahmad", "South Surra", "Kaifan", "Sharq"
        ];

        private static readonly string[] KuwaitiStreets =
        [
            "Gulf Road", "Arabian Gulf Street", "Salem Al-Mubarak Street",
            "Baghdad Street", "Tunis Street", "Damascus Street",
            "Beirut Street", "Mecca Street", "Fahaheel Expressway",
            "King Fahd Road", "Jamal Abdul Nasser Street",
            "First Ring Road", "Second Ring Road", "Third Ring Road",
            "Fourth Ring Road", "Fifth Ring Road", "Sixth Ring Road",
            "Coastal Road", "Airport Road"
        ];

        private static readonly string[] KuwaitiFirstNames =
        [
            "Mohammed", "Ahmed", "Abdullah", "Fahad", "Khalid", "Sultan",
            "Faisal", "Hamad", "Meshal", "Nawaf", "Talal", "Jassim",
            "Yousef", "Ali", "Hassan", "Omar", "Bader", "Nasser", "Saud",
            "Majed", "Nayef", "Thamer", "Bandar", "Rashid",
            "Fatima", "Maryam", "Aisha", "Noura", "Sarah", "Layla",
            "Hessa", "Shaikha", "Latifa", "Moza", "Amna", "Haya",
            "Dana", "Reem", "Lulwa", "Anoud"
        ];

        private static readonly string[] ArabicLastNames =
        [
            "Al-Mutairi", "Al-Ajmi", "Al-Rashidi", "Al-Dosari", "Al-Harbi",
            "Al-Shammari", "Al-Otaibi", "Al-Anzi", "Al-Qahtani", "Al-Ghanim",
            "Al-Sabah", "Al-Salem", "Al-Ahmad", "Al-Ali", "Al-Khaled",
            "Mohamed", "Ibrahim", "Hassan", "Hussein", "Abdullah",
            "Ismail", "Mansour", "Farouk", "Sayed", "Othman"
        ];

        private static readonly string[] FemaleNames =
        [
            "Fatima", "Maryam", "Aisha", "Noura", "Sarah", "Layla",
            "Hessa", "Shaikha", "Latifa", "Moza", "Amna", "Haya",
            "Dana", "Reem", "Lulwa", "Anoud", "Mariam", "Noor"
        ];

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ILogger<AppDataSeeder> _logger;
        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly IConfiguration _configuration;

        public AppDataSeeder(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ILogger<AppDataSeeder> logger,
            ITenantIdProvider tenantIdProvider,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _tenantIdProvider = tenantIdProvider;
            _configuration = configuration;
        }

        // IHostEnvironment isn't guaranteed available to a plain class library project (it's an
        // ASP.NET Core hosting abstraction) - ASPNETCORE_ENVIRONMENT is the same underlying value
        // IHostEnvironment.EnvironmentName reflects, and it's already reachable through the
        // IConfiguration this class already takes.
        private bool IsProductionEnvironment() =>
            string.Equals(_configuration["ASPNETCORE_ENVIRONMENT"], "Production", StringComparison.OrdinalIgnoreCase);

        // Cross-tenant shared/reference data that must exist in EVERY environment, including a
        // brand-new production database with zero demo data: roles, the Feature catalog, the
        // subscription-plan catalog (CreateTenantCommand needs at least one to reference), the
        // nationality-category lookup (every trainee registration needs one), and the SuperAdmin
        // account itself. Each of these already reconciles (add-missing, idempotent) rather than
        // assuming a fresh database, so calling this on every startup is safe indefinitely - the
        // same reasoning ReconcileFeaturesAsync already established for the Feature catalog.
        public async Task EnsureCoreDataAsync()
        {
            await SeedRolesAsync();
            var featureIds = await ReconcileFeaturesAsync();
            await ReconcileSubscriptionPlansAsync(featureIds);
            await ReconcileNationalityCategoriesAsync();
            await EnsureSystemTenantAndSuperAdminAsync();
        }

        // Demo-only: the fictional "Salmiya Academy" tenant and its full business dataset, plus a
        // test Accountant login. Never runs in Production (see Seeding:Enabled in
        // appsettings.Production.json / Program.cs) - real deployments call EnsureCoreDataAsync
        // above only, so the platform launches with the SuperAdmin and shared reference data
        // above but zero customer-shaped tenants.
        public async Task SeedDemoDataAsync()
        {
            // Must run unconditionally within this method (not just on a fresh database) - a
            // test Accountant login needs to exist even against an already-seeded dev database,
            // even though the rest of this method is about to no-op below.
            await EnsureTestAccountantUserAsync();

            if (await _context.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Code != Tenant.SystemTenantCode))
            {
                _logger.LogInformation("Demo tenant already seeded. Skipping demo/business-data seeding.");
                return;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("=== Starting Demo Data Seeding ===");

                var salmiyaTenantId = Guid.NewGuid();
                var ownerId = Guid.NewGuid();

                await SeedSalmiyaTenantAndOwnerAsync(salmiyaTenantId, ownerId);

                // Everything below creates tenant-scoped entities for Salmiya Academy -
                // TenantSaveChangesInterceptor stamps every newly-added ITenantScoped entity
                // with whatever the ambient tenant is on SaveChanges, so this must stay set to
                // salmiyaTenantId for the rest of this method.
                _tenantIdProvider.SetTenantId(salmiyaTenantId);

                var featureIds = await _context.Set<Feature>().Select(f => f.Id).ToListAsync();
                var enterprisePlanId = await _context.SubscriptionPlans
                    .Where(p => p.Code == "ENTERPRISE")
                    .Select(p => p.Id)
                    .FirstAsync();
                var natCatIds = await _context.NationalityCategories
                    .ToDictionaryAsync(c => c.Code, c => c.Id);

                await SeedTenantSettingsAsync(salmiyaTenantId, enterprisePlanId);
                await EnableTenantFeaturesAsync(salmiyaTenantId, featureIds);

                await SeedSalmiyaDataAsync(salmiyaTenantId, natCatIds);

                await transaction.CommitAsync();
                _logger.LogInformation("=== Demo Data Seeding Completed Successfully ===");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Demo data seeding failed. Transaction rolled back.");
                throw;
            }
        }

        // Idempotent and safe to retry from any partial-failure point - see the two guards below.
        // Creates the tenant row FIRST with a null OwnerId, then the user, then patches OwnerId -
        // this ordering (rather than creating both users this method's caller used to seed up
        // front and only wiring the FK afterward) means neither side of the Tenant<->AppUser
        // circular reference is ever inserted pointing at a row that doesn't exist yet, so no FK
        // enable/disable dance is needed (Tenant.OwnerId is nullable for exactly this reason -
        // CreateTenantCommandHandler leaves it null until invite-acceptance sets it too).
        private async Task EnsureSystemTenantAndSuperAdminAsync()
        {
            // Looked up by Code, not created unconditionally: if a prior attempt got this far
            // and committed the tenant but then failed on a later step (e.g. SuperAdmin:Password
            // didn't satisfy Identity's password policy), a naive "always insert a new Tenant"
            // here would crash every subsequent restart on IX_Tenants_Code's uniqueness instead
            // of ever recovering. Reusing the existing row makes this method safe to retry.
            var systemTenant = await _context.Tenants.IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Code == Tenant.SystemTenantCode);

            if (systemTenant is null)
            {
                systemTenant = new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = "System",
                    DisplayName = "System Platform",
                    Email = "system@sportacademy.com.kw",
                    Code = Tenant.SystemTenantCode,
                    Slug = "system",
                    Status = TenantStatus.Active,
                    OwnerId = null,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Tenants.Add(systemTenant);
                await _context.SaveChangesAsync();
            }

            // Must happen before the very next line, not after: AppUser is ITenantScoped, so
            // FindByEmailAsync is filtered by whatever the ambient tenant currently is. Checking
            // before this point (the previous version of this method did) filters on
            // TenantId == null - which no real user ever has - so it always reports "no existing
            // SuperAdmin" regardless of what's actually in the database. That's exactly what let
            // a prior failed attempt's already-created user go undetected here and crash with
            // "That username is already taken" inside CreateAsync below instead.
            _tenantIdProvider.SetTenantId(systemTenant.Id);

            var existingUser = await _userManager.FindByEmailAsync(SuperAdminEmail);
            if (existingUser is not null)
            {
                // Reconcile rather than assume a fully-finished prior run - a previous attempt
                // may have created the user but failed on the role assignment or the OwnerId
                // patch below.
                if (!await _userManager.IsInRoleAsync(existingUser, "SuperAdmin"))
                    await _userManager.AddToRoleAsync(existingUser, "SuperAdmin");
                if (systemTenant.OwnerId is null)
                {
                    systemTenant.OwnerId = existingUser.Id;
                    await _context.SaveChangesAsync();
                }
                // AppUser.PhoneNumber (from ASP.NET Identity, not a TenantProfile) is the
                // SuperAdmin's own account-level contact info, same field My Profile reads/writes
                // for any Owner/Admin/Accountant with no linked Employee/Trainee record - see
                // UpdateMyProfileCommand's own comment. Backfilled here for an already-bootstrapped
                // instance from before this existed, same reasoning as the OwnerId reconciliation
                // just above.
                if (string.IsNullOrWhiteSpace(existingUser.PhoneNumber))
                {
                    existingUser.PhoneNumber = SuperAdminPhoneNumber;
                    await _context.SaveChangesAsync();
                }
                return;
            }

            _logger.LogInformation("Bootstrapping SuperAdmin account...");

            // Same fail-fast shape as Program.cs's Cors:AllowedOrigins check: required in
            // Production (no safe default exists for a real deployment's admin password), but
            // falls back to the existing dev-only default elsewhere so local development isn't
            // broken by this requirement. Deliberately checked only once we know a SuperAdmin
            // actually needs creating - an already-bootstrapped instance should never demand this
            // on every subsequent restart.
            var password = _configuration["SuperAdmin:Password"];
            if (string.IsNullOrWhiteSpace(password))
            {
                if (IsProductionEnvironment())
                {
                    throw new InvalidOperationException(
                        "SuperAdmin:Password is not configured. Set it via the SuperAdmin__Password " +
                        "environment variable before starting the app - the SuperAdmin account " +
                        "cannot be bootstrapped without it.");
                }

                _logger.LogWarning(
                    "SuperAdmin:Password is not configured - using the default development " +
                    "password. This is only acceptable outside Production.");
                password = DefaultPassword;
            }

            var superAdmin = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = SuperAdminUserName,
                Email = SuperAdminEmail,
                PhoneNumber = SuperAdminPhoneNumber,
                TenantId = systemTenant.Id,
                IsPasswordReset = false,
                IsBanned = false,
                EmailConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = true
            };
            var result = await _userManager.CreateAsync(superAdmin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create SuperAdmin: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            _context.Profiles.Add(new Profile { AppUserId = superAdmin.Id });

            systemTenant.OwnerId = superAdmin.Id;
            await _context.SaveChangesAsync();

            await _userManager.AddToRoleAsync(superAdmin, "SuperAdmin");

            _logger.LogInformation("SuperAdmin bootstrapped successfully.");
        }

        // Same nullable-OwnerId-then-patch ordering as EnsureSystemTenantAndSuperAdminAsync above,
        // for the same reason - avoids needing the old FK-disable/re-enable dance for this demo
        // tenant too.
        private async Task SeedSalmiyaTenantAndOwnerAsync(Guid salmiyaTenantId, Guid ownerId)
        {
            _logger.LogInformation("Seeding Salmiya Academy tenant and owner...");

            var salmiyaTenant = new Tenant
            {
                Id = salmiyaTenantId,
                Name = "Salmiya Academy",
                DisplayName = "Salmiya Swimming Academy",
                Email = "info@salmiya-academy.com.kw",
                Code = "SALMYIA",
                Slug = "salmiya-academy",
                Status = TenantStatus.Active,
                OwnerId = null,
                CreatedAt = DateTime.UtcNow
            };
            _context.Tenants.Add(salmiyaTenant);
            await _context.SaveChangesAsync();

            _tenantIdProvider.SetTenantId(salmiyaTenantId);

            var owner = new AppUser
            {
                Id = ownerId,
                UserName = "mohammed.alatfy",
                Email = "mohammed.alatfy@salmiya-academy.com.kw",
                TenantId = salmiyaTenantId,
                IsPasswordReset = false,
                IsBanned = false,
                EmailConfirmed = true,
                PhoneNumber = "+96550775995",
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = true
            };
            var result = await _userManager.CreateAsync(owner, DefaultPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create Owner: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            _context.Profiles.Add(new Profile { AppUserId = owner.Id });

            salmiyaTenant.OwnerId = ownerId;
            await _context.SaveChangesAsync();

            await _userManager.AddToRoleAsync(owner, "Owner");

            _logger.LogInformation("Salmiya Academy tenant and owner seeded successfully.");
        }

        // Runs on every startup (see the call site in SeedAsync, before the early-return that
        // skips the rest of seeding once a tenant exists) so a demo Accountant login is always
        // available to exercise the expense/payroll permission boundary, even against an
        // already-seeded database. IgnoreQueryFilters here because no ambient tenant id is set
        // yet - same reason SeedAsync's own "any tenant exists" check above needs it.
        private async Task EnsureTestAccountantUserAsync()
        {
            var salmiyaTenant = await _context.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Slug == "salmiya-academy");

            if (salmiyaTenant is null)
                return;

            _tenantIdProvider.SetTenantId(salmiyaTenant.Id);

            var existing = await _userManager.FindByNameAsync("sonnet");
            if (existing is not null)
                return;

            var accountant = new AppUser
            {
                UserName = "sonnet",
                Email = "sonnet@claude.com",
                TenantId = salmiyaTenant.Id,
                IsPasswordReset = false,
                IsBanned = false,
                EmailConfirmed = true,
                PhoneNumber = "+96550775996",
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = true
            };

            var result = await _userManager.CreateAsync(accountant, "clAude@123");
            if (!result.Succeeded)
            {
                _logger.LogWarning(
                    "Failed to create test Accountant user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            _context.Profiles.Add(new Profile { AppUserId = accountant.Id });
            await _context.SaveChangesAsync();

            await _userManager.AddToRoleAsync(accountant, "Accountant");
        }

        // Default permission grants per seeded role - the tenant business model has exactly
        // four roles (Owner, Admin, Employee, Accountant) plus the platform-only SuperAdmin.
        // Owner and Admin share every tenant-business permission except tenant.users.manage,
        // which is Owner-only (see ConsolidateRolesToFour migration for why: Admin managing
        // its own permission ceiling would be a privilege-escalation hole). Kept in sync with
        // the identical list baked into that migration's SQL, since the seeder below is a
        // no-op on any database that already has a tenant (see SeedAsync's early return) and
        // so cannot be relied on to fix an existing deployment's role claims.
        private static readonly Dictionary<string, string[]> DefaultRolePermissions = new()
        {
            ["SuperAdmin"] = [.. Permissions.All],
            ["Owner"] = [.. Permissions.All.Where(p => !p.StartsWith("platform."))],
            ["Admin"] = [.. Permissions.All.Where(p => !p.StartsWith("platform.") && p != Permissions.Tenant.ManageUsers)],
            ["Employee"] =
            [
                Permissions.Trainee.Register, Permissions.Trainee.Edit, Permissions.Trainee.Export,
                Permissions.Enrollment.Create, Permissions.Enrollment.Edit, Permissions.Enrollment.Activate,
                Permissions.Subscription.Manage,
                // TraineeGroup.Create is deliberately NOT granted here - creating a new group is
                // Owner/Admin only. Employee still keeps Manage (update/delete/pause/resume) for
                // groups that already exist.
                Permissions.TraineeGroup.Manage, Permissions.TraineeGroup.GenerateSessions, Permissions.Session.Manage,
                Permissions.Attendance.Mark, Permissions.Attendance.ViewRate,
                Permissions.Report.ViewAttendance, Permissions.Report.ViewSubscriptions,
            ],
            ["Accountant"] =
            [
                Permissions.Payment.Record, Permissions.Payment.Correct, Permissions.Payment.Refund, Permissions.Payment.View,
                Permissions.Finance.View,
                Permissions.Report.View, Permissions.Report.Export,
                Permissions.Trainee.Export,
                // Accountant runs payroll end-to-end except the approval gate itself
                // (Salary.Approve) - that's deliberately Owner/Admin-only, an accountant who
                // creates a salary payment must not also be able to approve their own request.
                Permissions.Expense.Manage, Permissions.Expense.View,
                Permissions.Salary.View, Permissions.Salary.Create, Permissions.Salary.MarkPaid,
                // Accountant never holds Subscription.Manage (can't create a subscription), so
                // it can never be the requester of a discount-subscription request - safe to let
                // it approve/reject one too, alongside Owner/Admin. DiscountCode.Manage
                // (minting codes) is deliberately NOT granted here - Owner/Admin only.
                Permissions.DiscountCode.Approve,
            ],
            // Platform-level, read-only: sees tenant/audit data across the whole platform but
            // cannot mutate a tenant, ban an owner, or impersonate into one - a support/success
            // seat distinct from SuperAdmin, which holds every platform.* permission. No UI
            // currently exists to assign this role to a second platform user (there is no
            // "platform users" management page anywhere in the app) - it exists in the seeder
            // and can be granted by hand (e.g. a DB script or a future admin UI) but has no
            // self-service provisioning path yet.
            ["PlatformSupport"] = [Permissions.Platform.TenantsRead, Permissions.Platform.AuditRead],
        };

        private async Task SeedRolesAsync()
        {
            _logger.LogInformation("Seeding roles...");

            var roleNames = new[] { "SuperAdmin", "Owner", "Admin", "Employee", "Accountant", "PlatformSupport" };
            foreach (var roleName in roleNames)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role is null)
                {
                    role = new AppRole { Name = roleName };
                    var result = await _roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                        throw new InvalidOperationException($"Failed to create role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                await SeedRolePermissionsAsync(role, DefaultRolePermissions.GetValueOrDefault(roleName, []));
            }

            await RemoveObsoleteRolesAsync(roleNames);

            _logger.LogInformation("Roles seeded successfully.");
        }

        // Mirrors SeedRolePermissionsAsync's reconciliation, but for roles themselves - that
        // method only ever adds a role from roleNames, so a role dropped from that list (e.g.
        // the old Manager/Coach/User roles the ConsolidateRolesToFour migration once collapsed)
        // would otherwise linger in the database forever with no automatic cleanup. Only ever
        // deletes a role that currently has nobody assigned to it: reassigning real users out of
        // a role being removed is a business decision (see ConsolidateRolesToFour's explicit
        // Manager->Admin / Coach->Employee / User->Employee mapping) that this generic
        // reconciliation has no safe way to make on its own, so an obsolete role still in use is
        // left in place with a warning instead of guessed at.
        private async Task RemoveObsoleteRolesAsync(string[] desiredRoleNames)
        {
            var desired = desiredRoleNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var obsoleteRoles = await _roleManager.Roles
                .Where(r => r.Name != null && !desired.Contains(r.Name))
                .ToListAsync();

            foreach (var role in obsoleteRoles)
            {
                // Not _userManager.GetUsersInRoleAsync - it joins through AppUser, which is
                // ITenantScoped, and the ambient tenant is never set to anything at this point
                // in startup (SeedRolesAsync runs before any SetTenantId call), so that join's
                // global query filter would silently see zero users in EVERY tenant regardless
                // of the truth, defeating this entire safety check. AppUserRole itself carries
                // no TenantId, so querying it directly is unaffected by that filter.
                var usersInRoleCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
                if (usersInRoleCount > 0)
                {
                    _logger.LogWarning(
                        "Role {RoleName} is no longer in the seeded role list but still has {UserCount} " +
                        "user(s) assigned - leaving it in place. Reassign those users to a current role and " +
                        "it will be removed automatically on the next startup.",
                        role.Name, usersInRoleCount);
                    continue;
                }

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    _logger.LogInformation("Removed obsolete, unused role {RoleName}.", role.Name);
                else
                    _logger.LogWarning("Failed to remove obsolete role {RoleName}: {Errors}",
                        role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Reconciles rather than just adds: also removes any "permission" claim the role
        // carries that is no longer in its default set. Without this, a permission once
        // granted to a role (e.g. Admin's old tenant.users.manage) would never be revocable by
        // changing DefaultRolePermissions and restarting - it would linger on every role that
        // was ever seeded with it. (In practice this only matters for a fresh database that
        // runs SeedAsync more than once in its lifetime with different code, e.g. tests; a real
        // deployment's roles are fixed up by the ConsolidateRolesToFour migration instead,
        // since SeedAsync itself is a no-op once a tenant exists.)
        private async Task SeedRolePermissionsAsync(AppRole role, string[] permissions)
        {
            var desired = permissions.ToHashSet();
            var existingClaims = (await _roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == "permission")
                .ToList();
            var existing = existingClaims.Select(c => c.Value).ToHashSet();

            foreach (var permission in desired.Except(existing))
                await _roleManager.AddClaimAsync(role, new Claim("permission", permission));

            foreach (var claim in existingClaims.Where(c => !desired.Contains(c.Value)))
                await _roleManager.RemoveClaimAsync(role, claim);
        }

        // Single source of truth for the Feature catalog, shared by SeedFeaturesAsync (fresh
        // database) and ReconcileFeaturesAsync (already-seeded database) so the two paths can
        // never drift apart. trainee-reports / coach-reports / operational-reports remain
        // absent: there is no ReportsController support for them yet (only the financial/
        // attendance/subscription reports below are implemented), and listing them here would
        // let a SuperAdmin toggle on a feature no tenant can actually use. Add each once its
        // report implementation lands.
        private static readonly (string Name, string DisplayName, string Description)[] FeatureCatalog =
        [
            ("user-management", "User Management", "Create, edit, and manage system users"),
            ("role-management", "Role & Permission Management", "Define roles and assign permissions"),
            ("tenant-settings", "Tenant Configuration", "Configure tenant-wide settings"),
            ("branch-management", "Branch Management", "Manage academy branches and locations"),
            ("trainee-management", "Trainee Management", "Register and manage trainee profiles"),
            ("employee-management", "Employee Management", "Manage staff and employee records"),
            ("coach-management", "Coach Management", "Assign and manage coaches"),
            ("sport-management", "Sports Management", "Define sports and training activities"),
            ("subscription-plan", "Subscription Plans", "Create and manage subscription offerings"),
            ("pricing-management", "Pricing Management", "Set sport and branch pricing"),
            ("payment-processing", "Payment Processing", "Process and track payments"),
            ("group-management", "Group Management", "Form and manage training groups"),
            ("schedule-management", "Schedule Management", "Create class schedules and timetables"),
            ("attendance-tracking", "Attendance Tracking", "Record and report attendance"),
            ("enrollment-management", "Enrollment Management", "Manage trainee enrollments"),
            ("family-management", "Family Management", "Manage family accounts and billing"),
            ("nationality-categories", "Nationality Categories", "Configure nationality classifications"),
            ("financial-reports", "Financial Reports", "Revenue, outstanding, and payment-method reports"),
            ("attendance-reports", "Attendance Reports", "Full attendance history, filterable and printable"),
            ("subscription-reports", "Subscription Reports", "Full subscription history, filterable and printable"),
            ("notifications", "Notification System", "Send and manage system notifications"),
            ("chat-system", "In-App Chat", "Internal messaging and communication"),
            ("health-test-mgmt", "Health Test Management", "Track health assessments and tests"),
            ("discount-offers", "Discounts & Offers", "Manage promotions and discounts"),
            ("session-management", "Session Management", "Manage training sessions"),
            ("audit-trail", "Audit Trail", "System activity logging"),
            ("system-settings", "System Settings", "Global system configuration"),
            ("profile-mgmt", "Profile Management", "User profile and preferences"),
            ("ai-assistant", "AI Assistant", "AI-powered help and insights"),
            ("api-access", "API Access", "External API integration management"),
            ("backup-restore", "Backup & Restore", "Data backup and restoration"),
            ("trainee-codes", "Trainee Code Management", "Custom trainee code assignment"),
        ];

        // Fully replaces the old "SeedFeaturesAsync always inserts everything, assumes it only
        // ever runs once" approach. Adds any Feature this catalog defines that the database
        // doesn't have yet (on a genuinely fresh database, that's every feature - this is what
        // populates Features now) and enables each newly-added one by default for every
        // pre-existing tenant, matching what a fresh seed of that tenant would already produce.
        // A tenant seeded later in this same call (the fresh-DB path) is deliberately excluded
        // from that enablement loop - it gets every current feature via its own
        // EnableTenantFeaturesAsync call further down in SeedAsync, using the ids returned here.
        // Must run unconditionally on every startup (see the call site in SeedAsync), or a
        // feature added to FeatureCatalog after go-live would never reach an already-seeded
        // deployment without a full drop/recreate - the same problem SeedRolePermissionsAsync
        // already solves for permissions.
        private async Task<List<Guid>> ReconcileFeaturesAsync()
        {
            var existing = await _context.Set<Feature>().ToListAsync();
            var existingNames = existing.Select(f => f.Name).ToHashSet();

            var missing = FeatureCatalog
                .Where(f => !existingNames.Contains(f.Name))
                .Select(f => CreateFeature(f.Name, f.DisplayName, f.Description))
                .ToList();

            if (missing.Count > 0)
            {
                _logger.LogInformation("Reconciling {Count} new feature(s) into the catalog: {Names}",
                    missing.Count, string.Join(", ", missing.Select(f => f.Name)));
                _context.Set<Feature>().AddRange(missing);
                await _context.SaveChangesAsync();

                var tenantIds = await _context.Tenants.IgnoreQueryFilters().Select(t => t.Id).ToListAsync();
                if (tenantIds.Count > 0)
                {
                    var now = DateTime.UtcNow;
                    foreach (var tenantId in tenantIds)
                    {
                        foreach (var feature in missing)
                        {
                            _context.TenantFeatures.Add(new TenantFeature
                            {
                                TenantId = tenantId,
                                FeatureId = feature.Id,
                                IsEnabled = true,
                                EnabledAt = now,
                                EnabledBy = "System",
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("New feature(s) enabled for {Count} existing tenant(s).", tenantIds.Count);
                }
            }

            return existing.Select(f => f.Id).Concat(missing.Select(f => f.Id)).ToList();
        }

        private static Feature CreateFeature(string name, string displayName, string description)
        {
            return new Feature
            {
                Id = Guid.NewGuid(),
                Name = name,
                DisplayName = displayName,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static readonly (string Name, string Code, string Description, decimal MonthlyPrice, decimal YearlyPrice)[] SubscriptionPlanCatalog =
        [
            ("Basic", "BASIC", "Core features for small academies", 49, 499),
            ("Professional", "PRO", "Full feature set for growing academies", 99, 999),
            ("Enterprise", "ENTERPRISE", "Complete suite with AI and advanced analytics", 199, 1999)
        ];

        // Same reconciliation shape as ReconcileFeaturesAsync above (add-missing-by-Code, safe on
        // every startup) - this is the platform-wide plan catalog CreateTenantCommand references,
        // so it must exist before a SuperAdmin can create the very first real tenant.
        private async Task ReconcileSubscriptionPlansAsync(List<Guid> featureIds)
        {
            var existing = await _context.SubscriptionPlans.ToListAsync();
            var existingCodes = existing.Select(p => p.Code).ToHashSet();

            var missing = SubscriptionPlanCatalog
                .Where(p => !existingCodes.Contains(p.Code))
                .Select(p => new SubscriptionPlan
                {
                    Name = p.Name,
                    Code = p.Code,
                    Description = p.Description,
                    MonthlyPrice = p.MonthlyPrice,
                    YearlyPrice = p.YearlyPrice,
                    IsActive = true
                })
                .ToList();

            if (missing.Count == 0)
                return;

            _logger.LogInformation("Reconciling {Count} new subscription plan(s): {Codes}",
                missing.Count, string.Join(", ", missing.Select(p => p.Code)));
            _context.SubscriptionPlans.AddRange(missing);
            await _context.SaveChangesAsync();

            var basicFeatures = featureIds.Take(15).ToList();
            var professionalFeatures = featureIds.Take(28).ToList();
            var enterpriseFeatures = featureIds.ToList();

            foreach (var plan in missing)
            {
                var assignedFeatures = plan.Code switch
                {
                    "BASIC" => basicFeatures,
                    "PRO" => professionalFeatures,
                    "ENTERPRISE" => enterpriseFeatures,
                    _ => professionalFeatures
                };

                foreach (var featureId in assignedFeatures)
                {
                    _context.SubscriptionPlanFeatures.Add(new SubscriptionPlanFeature
                    {
                        SubscriptionPlanId = plan.Id,
                        FeatureId = featureId
                    });
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Subscription plan catalog reconciled successfully.");
        }

        private static readonly (string Code, string Name)[] NationalityCategoryCatalog =
        [
            ("KW", "Kuwaiti"), ("GCC", "GCC National"), ("AR", "Arab (Non-GCC)"),
            ("AS", "Asian"), ("AF", "African"), ("EU", "European"),
            ("NA", "North American"), ("SA", "South American"), ("OT", "Other")
        ];

        // Same reconciliation shape again - every tenant's trainee registration form needs this
        // lookup populated (Trainee.NationalityCategoryId), so it must exist unconditionally too.
        private async Task ReconcileNationalityCategoriesAsync()
        {
            var existing = await _context.NationalityCategories.ToListAsync();
            var existingCodes = existing.Select(c => c.Code).ToHashSet();

            var missing = NationalityCategoryCatalog
                .Where(c => !existingCodes.Contains(c.Code))
                .Select(c => new NationalityCategory { Code = c.Code, Name = c.Name })
                .ToList();

            if (missing.Count == 0)
                return;

            _logger.LogInformation("Reconciling {Count} new nationality categor(y/ies): {Codes}",
                missing.Count, string.Join(", ", missing.Select(c => c.Code)));
            _context.NationalityCategories.AddRange(missing);
            await _context.SaveChangesAsync();
        }

        private async Task SeedTenantSettingsAsync(Guid tenantId, int planId)
        {
            _logger.LogInformation("Seeding tenant settings for Salmiya Academy...");

            _context.TenantProfiles.Add(new TenantProfile
            {
                TenantId = tenantId,
                OrganizationName = "Salmiya Swimming Academy",
                Email = "info@salmiya-academy.com.kw",
                Phone = "+965 1800080",
                Address = "Gulf Road, Salmiya, Kuwait",
                Description = "Premier swimming and sports academy located in the heart of Salmiya, Kuwait. Offering world-class training facilities for all ages and skill levels.",
                CommercialRegistration = "CR-2024-SALM-001"
            });

            _context.TenantSettings.Add(new TenantSettings
            {
                TenantId = tenantId,
                TimeZone = "Asia/Kuwait",
                Language = "ar-KW",
                DateFormat = "dd/MM/yyyy",
                TimeFormat = "HH:mm",
                Currency = "KWD"
            });

            _context.TenantSubscriptions.Add(new TenantSubscription
            {
                TenantId = tenantId,
                StartsAt = DateTime.UtcNow,
                EndsAt = DateTime.UtcNow.AddYears(1),
                IsTrial = false,
                AutoRenew = true,
                SubscriptionPlanId = planId
            });

            await _context.SaveChangesAsync();
            _logger.LogInformation("Tenant settings seeded successfully.");
        }

        private async Task EnableTenantFeaturesAsync(Guid tenantId, List<Guid> featureIds)
        {
            _logger.LogInformation("Enabling features for Salmiya Academy...");

            var now = DateTime.UtcNow;
            foreach (var featureId in featureIds)
            {
                _context.TenantFeatures.Add(new TenantFeature
                {
                    TenantId = tenantId,
                    FeatureId = featureId,
                    IsEnabled = true,
                    EnabledAt = now,
                    EnabledBy = "System"
                });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Features enabled successfully.");
        }

        private async Task SeedSalmiyaDataAsync(Guid tenantId, Dictionary<string, int> natCats)
        {
            _logger.LogInformation("=== Seeding Salmiya Academy Domain Data ===");

            var faker = new Faker("en");
            var random = new Random();

            var branches = CreateBranches(tenantId);
            _context.Branchs.AddRange(branches);
            await _context.SaveChangesAsync();

            var sports = CreateSports(tenantId);
            _context.Sports.AddRange(sports);
            await _context.SaveChangesAsync();

            var subTypes = CreateSubscriptionTypes(tenantId);
            _context.SubscriptionTypes.AddRange(subTypes);
            await _context.SaveChangesAsync();

            var branchIds = branches.Select(b => b.Id).ToList();
            var sportIds = sports.Select(s => s.Id).ToList();
            var subTypeIds = subTypes.Select(st => st.Id).ToList();

            var sportBranches = CreateSportBranches(tenantId, sports, branches);
            _context.Set<SportBranch>().AddRange(sportBranches);
            await _context.SaveChangesAsync();

            var sportSubTypes = CreateSportSubscriptionTypes(tenantId, sports, subTypes);
            _context.Set<SportSubscriptionType>().AddRange(sportSubTypes);
            await _context.SaveChangesAsync();

            var basePrices = new Dictionary<string, decimal>
            {
                ["Swimming"] = 60m,
                ["Football"] = 45m,
                ["Basketball"] = 40m,
                ["Volleyball"] = 35m,
                ["Tennis"] = 55m,
                ["Martial Arts"] = 50m,
                ["Gymnastics"] = 65m,
                ["Table Tennis"] = 30m
            };
            var sportPriceLookup = sports.ToDictionary(s => s.Id, s => basePrices.GetValueOrDefault(s.Name, 40m));
            var sportPrices = CreateSportPrices(tenantId, sportBranches, sportPriceLookup, subTypes);
            _context.Set<SportPrice>().AddRange(sportPrices);
            await _context.SaveChangesAsync();

            var employees = await CreateEmployeesAsync(tenantId, branches);
            _context.Employees.AddRange(employees);
            await _context.SaveChangesAsync();

            var coachEmployees = employees.Where(e => e.Position == Position.Coach).ToList();
            var coaches = CreateCoaches(coachEmployees, sports, tenantId);
            _context.Set<Coach>().AddRange(coaches);
            await _context.SaveChangesAsync();

            var families = CreateFamilies(tenantId, 30);
            _context.Families.AddRange(families);
            await _context.SaveChangesAsync();

            var trainees = CreateTrainees(tenantId, branches, natCats, families, faker, random);
            _context.Trainees.AddRange(trainees);
            await _context.SaveChangesAsync();

            var traineeGroups = CreateTraineeGroups(tenantId, branches, coaches, faker, random);
            _context.TraineeGroups.AddRange(traineeGroups);
            await _context.SaveChangesAsync();

            var groupSchedules = CreateGroupSchedules(tenantId, traineeGroups, random);
            _context.Set<GroupSchedule>().AddRange(groupSchedules);
            await _context.SaveChangesAsync();

            var paymentTypes = CreatePaymentTypes(tenantId);
            _context.Set<PaymentType>().AddRange(paymentTypes);
            await _context.SaveChangesAsync();

            var payments = CreatePayments(tenantId, branches, paymentTypes, random);
            _context.Set<Payment>().AddRange(payments);
            await _context.SaveChangesAsync();

            var coachSportById = coaches.ToDictionary(c => c.EmployeeId, c => c.SportId);
            var subscriptionDetails = CreateSubscriptionDetails(
                tenantId, trainees, subTypes, sportBranches, traineeGroups, groupSchedules,
                coachSportById, payments, random);
            _context.Set<SubscriptionDetails>().AddRange(subscriptionDetails);
            await _context.SaveChangesAsync();

            // Every seeded subscription is billed via an Invoice (see Finance.Invoice) rather
            // than fabricating a Payment for it - money is now a deliberate, separate act, and
            // roughly half of these demo invoices are left unpaid so the Accountant console has
            // something to show under "outstanding".
            var invoices = CreateSeedInvoices(tenantId, subscriptionDetails, random);
            _context.Set<Domain.Entities.Finance.Invoice>().AddRange(invoices);
            await _context.SaveChangesAsync();

            var allocations = AllocateSeedPayments(subscriptionDetails, invoices, payments, random);
            _context.Set<Domain.Entities.Finance.PaymentAllocation>().AddRange(allocations);
            await _context.SaveChangesAsync();

            // CreateSeedInvoices assigns InvoiceNumbers directly (INV-{year}-00001, 00002, ...)
            // rather than going through usp_GenerateDocumentNumber, so the counter it backs
            // (DocumentNumberCounters) doesn't know those numbers were ever handed out. Without
            // this, the first real invoice issued after seeding starts back at 00001 and
            // collides with the seed data's own INV-{year}-00001. Advance the counter to match
            // what was actually seeded so real usage picks up cleanly after it.
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                MERGE DocumentNumberCounters AS target
                USING (SELECT {tenantId} AS TenantId, N'INV' AS DocumentType, {DateTime.UtcNow.Year} AS [Year]) AS src
                    ON target.TenantId = src.TenantId AND target.DocumentType = src.DocumentType AND target.[Year] = src.[Year]
                WHEN MATCHED THEN UPDATE SET LastNumber = {invoices.Count}
                WHEN NOT MATCHED THEN INSERT (TenantId, DocumentType, [Year], LastNumber) VALUES (src.TenantId, src.DocumentType, src.[Year], {invoices.Count});
            ");

            var (enrollments, sportTrainees) = CreateEnrollments(tenantId, trainees, traineeGroups, coaches, subTypes, subscriptionDetails, random);
            _context.Set<SportTrainee>().AddRange(sportTrainees);
            _context.Enrollments.AddRange(enrollments);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Salmiya Academy domain data seeded successfully.");
        }

        private static List<Branch> CreateBranches(Guid tenantId)
        {
            var branchData = new[]
            {
                ("Salmiya Academy - Main Branch", "Salmiya", "+965 1800081", "main@salmiya-academy.com.kw", "29.3333", "48.0833"),
                ("Salmiya Academy - Hawally Branch", "Hawally", "+965 1800082", "hawally@salmiya-academy.com.kw", "29.3325", "48.0017"),
                ("Salmiya Academy - Jabriya Branch", "Jabriya", "+965 1800083", "jabriya@salmiya-academy.com.kw", "29.3258", "48.0583")
            };

            return branchData.Select((data, idx) => new Branch
            {
                Name = data.Item1,
                City = data.Item2,
                Country = "Kuwait",
                PhoneNumber = data.Item3,
                Email = data.Item4,
                CoX = data.Item5,
                CoY = data.Item6,
                IsActive = true,
                TenantId = tenantId
            }).ToList();
        }

        private static List<Sport> CreateSports(Guid tenantId)
        {
            var sportData = new[]
            {
                ("Swimming", "Professional swimming lessons and water safety training for all ages", SportCategory.Individual, true),
                ("Football", "The beautiful game - soccer training and competitive play", SportCategory.Team, true),
                ("Basketball", "Indoor basketball training and competitive play", SportCategory.Team, true),
                ("Volleyball", "Beach and indoor volleyball training", SportCategory.Team, false),
                ("Tennis", "Professional tennis coaching for all skill levels", SportCategory.Individual, false),
                ("Martial Arts", "Karate, Taekwondo, and self-defense training", SportCategory.Individual, true),
                ("Gymnastics", "Artistic gymnastics and flexibility training", SportCategory.Individual, true),
                ("Table Tennis", "Ping pong training and tournaments", SportCategory.Individual, false)
            };

            return sportData.Select(s => new Sport
            {
                Name = s.Item1,
                Description = s.Item2,
                Category = s.Item3,
                IsRequireHealthTest = s.Item4,
                TenantId = tenantId
            }).ToList();
        }

        private static List<SubscriptionType> CreateSubscriptionTypes(Guid tenantId)
        {
            return new List<SubscriptionType>
            {
                new() { Name = "Monthly", DaysPerMonth = 8, NumberOfMonths = 1, IsActive = true, IsOffer = false, TenantId = tenantId },
                new() { Name = "Quarterly", DaysPerMonth = 10, NumberOfMonths = 3, IsActive = true, IsOffer = false, TenantId = tenantId },
                new() { Name = "Silver", DaysPerMonth = 12, NumberOfMonths = 1, IsActive = true, IsOffer = false, TenantId = tenantId },
                new() { Name = "Gold", DaysPerMonth = 16, NumberOfMonths = 1, IsActive = true, IsOffer = false, TenantId = tenantId },
                new() { Name = "Platinum", DaysPerMonth = 24, NumberOfMonths = 1, IsActive = true, IsOffer = true, TenantId = tenantId }
            };
        }

        private static List<SportBranch> CreateSportBranches(Guid tenantId, List<Sport> sports, List<Branch> branches)
        {
            var result = new List<SportBranch>();
            var random = new Random();

            foreach (var branch in branches)
            {
                var count = random.Next(4, 7);
                foreach (var sport in sports.OrderBy(_ => random.Next()).Take(count))
                {
                    result.Add(new SportBranch
                    {
                        SportId = sport.Id,
                        BranchId = branch.Id,
                        IsAvailable = true,
                        TenantId = tenantId
                    });
                }
            }

            return result;
        }

        private static List<SportSubscriptionType> CreateSportSubscriptionTypes(Guid tenantId, List<Sport> sports, List<SubscriptionType> subTypes)
        {
            return sports.SelectMany(sport =>
                subTypes.Select(st => new SportSubscriptionType
                {
                    SportId = sport.Id,
                    SubscriptionTypeId = st.Id,
                    TenantId = tenantId
                })
            ).ToList();
        }

        private static List<SportPrice> CreateSportPrices(
            Guid tenantId, List<SportBranch> sportBranches, Dictionary<int, decimal> sportPriceLookup,
            List<SubscriptionType> subTypes)
        {
            var priceMultipliers = new Dictionary<string, decimal>
            {
                ["Monthly"] = 1.0m,
                ["Quarterly"] = 2.8m,
                ["Silver"] = 1.4m,
                ["Gold"] = 1.8m,
                ["Platinum"] = 2.5m
            };

            var random = new Random();
            var result = new List<SportPrice>();

            foreach (var sb in sportBranches)
            {
                var basePrice = sportPriceLookup.GetValueOrDefault(sb.SportId, 40m);
                foreach (var st in subTypes)
                {
                    var multiplier = priceMultipliers.GetValueOrDefault(st.Name, 1.0m);
                    var publicPrice = Math.Round(basePrice * multiplier + random.Next(-5, 5), 2);

                    // Both group types, because GroupType is part of the price's key: without a
                    // Private row here, picking Private in the subscription form fails
                    // validation with "no price configured" and the feature can't be exercised
                    // at all on a seeded database. Private is a small-group product, priced at a
                    // premium over the same plan's public price.
                    result.Add(new SportPrice
                    {
                        SportId = sb.SportId,
                        BranchId = sb.BranchId,
                        SubsTypeId = st.Id,
                        GroupType = TraineeGroupType.Public,
                        Price = publicPrice,
                        TenantId = tenantId
                    });

                    result.Add(new SportPrice
                    {
                        SportId = sb.SportId,
                        BranchId = sb.BranchId,
                        SubsTypeId = st.Id,
                        GroupType = TraineeGroupType.Private,
                        Price = Math.Round(publicPrice * PrivatePriceMultiplier, 2),
                        TenantId = tenantId
                    });
                }
            }

            return result;
        }

        private async Task<List<Employee>> CreateEmployeesAsync(Guid tenantId, List<Branch> branches)
        {
            var employees = new List<(string First, string Last, Position Position, Gender Gender, int BranchIndex)>
            {
                ("Ahmed", "Al-Mutairi", Position.Manager, Gender.Male, 0),
                ("Khalid", "Al-Ajmi", Position.Coach, Gender.Male, 0),
                ("Fahad", "Al-Rashidi", Position.Coach, Gender.Male, 0),
                ("Nasser", "Al-Otaibi", Position.Manager, Gender.Male, 1),
                ("Sultan", "Al-Shammari", Position.Coach, Gender.Male, 1),
                ("Hamad", "Al-Dosari", Position.Coach, Gender.Male, 1),
                ("Abdullah", "Al-Harbi", Position.Manager, Gender.Male, 2),
                ("Meshal", "Al-Anzi", Position.Coach, Gender.Male, 2),
                ("Yousef", "Al-Qahtani", Position.Coach, Gender.Male, 2),
                ("Omar", "Al-Ghanim", Position.Accountant, Gender.Male, 0),
                ("Nawaf", "Al-Sabah", Position.Coach, Gender.Male, 0),
                ("Bandar", "Al-Salem", Position.Coach, Gender.Male, 1),
                ("Ali", "Ibrahim", Position.HR, Gender.Male, 0),
                ("Hassan", "Mansour", Position.IT, Gender.Male, 1),
                ("Noura", "Al-Ahmad", Position.Accountant, Gender.Female, 2)
            };

            var random = new Random();
            var result = new List<Employee>();
            int userIndex = 0;
            var tenantUsers = await _userManager.Users
                .Where(u => u.TenantId == tenantId)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            foreach (var emp in employees)
            {
                var branch = branches[emp.BranchIndex < branches.Count ? emp.BranchIndex : 0];
                var employee = new Employee
                {
                    FirstName = emp.First,
                    LastName = emp.Last,
                    SSN = GenerateKuwaitiSSN(random, 1970, 2000),
                    BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-random.Next(22, 55))),
                    Gender = emp.Gender,
                    Nationality = emp.Gender == Gender.Female && emp.Last.StartsWith("Al-")
                        ? Nationality.Kuwaiti
                        : Nationality.Kuwaiti,
                    PhoneNumber = GenerateKuwaitiPhone(random),
                    SecondPhoneNumber = random.NextDouble() < 0.3 ? GenerateKuwaitiPhone(random) : null,
                    Address = Address.Create($"Street {random.Next(1, 250)}, Block {random.Next(1, 12)}", branch.City),
                    Email = Email.Create($"{emp.First.ToLower()}.{emp.Last.ToLower()}@salmiya-academy.com.kw"),
                    Salary = random.Next(400, 1500),
                    HireDate = DateTime.UtcNow.AddDays(-random.Next(30, 1095)),
                    Position = emp.Position,
                    IsWork = true,
                    BranchId = branch.Id,
                    TenantId = tenantId,
                    AppUserId = userIndex < tenantUsers.Count ? tenantUsers[userIndex++].Id : null
                };

                result.Add(employee);
            }

            return result;
        }

        private static List<Coach> CreateCoaches(List<Employee> coachEmployees, List<Sport> sports, Guid tenantId)
        {
            var random = new Random();
            return coachEmployees.Select(emp => new Coach
            {
                SkillLevel = random.Next(0, 2) == 0 ? SkillLevel.Beginner : (SkillLevel)random.Next(1, 4),
                Rate = random.Next(1, 5),
                EmployeeId = emp.Id,
                SportId = sports[random.Next(sports.Count)].Id,
                TenantId = tenantId
            }).ToList();
        }

        private static List<Family> CreateFamilies(Guid tenantId, int count)
        {
            return Enumerable.Range(1, count).Select(i => new Family
            {
                FamilyCode = i,
                LastMemberNumber = 0,
                TenantId = tenantId
            }).ToList();
        }

        private List<Trainee> CreateTrainees(
            Guid tenantId, List<Branch> branches,
            Dictionary<string, int> natCats, List<Family> families,
            Faker faker, Random random)
        {
            var trainees = new List<Trainee>();
            var usedIds = new HashSet<int>();

            for (int i = 0; i < 30; i++)
            {
                var isMale = random.Next(2) == 0;
                var firstName = isMale
                    ? faker.PickRandom(KuwaitiFirstNames.Where(n => !FemaleNames.Contains(n)).ToList())
                    : faker.PickRandom(FemaleNames);
                var lastName = faker.PickRandom(ArabicLastNames);
                var branch = branches[random.Next(branches.Count)];
                var natCat = natCats.ElementAt(random.Next(natCats.Count));
                var family = families[i];
                var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-random.Next(6, 35)));

                var trainee = new Trainee
                {
                    FirstName = firstName,
                    LastName = lastName,
                    SSN = GenerateKuwaitiSSN(random, 2000, 2018),
                    BirthDate = birthDate,
                    Gender = isMale ? Gender.Male : Gender.Female,
                    Nationality = Nationality.Kuwaiti,
                    PhoneNumber = GenerateKuwaitiPhone(random),
                    SecondPhoneNumber = random.NextDouble() < 0.2 ? GenerateKuwaitiPhone(random) : null,
                    Address = Address.Create($"Street {random.Next(1, 250)}, Block {random.Next(1, 12)}", branch.City),
                    Email = Email.Create($"{firstName.ToLower()}.{lastName.ToLower()}{random.Next(10, 999)}@email.com"),
                    TenantId = tenantId,
                    JoinDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-random.Next(30, 730))),
                    BranchId = branch.Id,
                    FamilyId = family.Id,
                    NationalityCategoryId = natCat.Value,
                    ParentNumber = null,
                    GuardianName = null
                };

                var age = trainee.GetAge();
                if (age < 15)
                {
                    trainee.ParentNumber = GenerateKuwaitiPhone(random);
                    trainee.GuardianName = $"{faker.PickRandom(KuwaitiFirstNames)} {faker.PickRandom(ArabicLastNames)}";
                }

                var memberNum = family.LastMemberNumber + 1;
                family.LastMemberNumber = memberNum;

                trainee.TraineeCode = TraineeCode.Create(
                    trainee.AgeCategory, family.FamilyCode, branch.Id,
                    natCat.Key, memberNum);

                var uniqueId = GenerateUniqueTraineeId(trainee, branch.Id, random, usedIds);
                trainee.Id = uniqueId;

                trainees.Add(trainee);
            }

            return trainees;
        }

        private static int GenerateUniqueTraineeId(Trainee trainee, int branchId, Random random, HashSet<int> usedIds)
        {
            int id;
            do
            {
                var year = trainee.BirthDate.Year % 100;
                var month = trainee.BirthDate.Month;
                var firstLetter = char.ToUpper(trainee.FirstName[0]);
                var ascii = ((int)firstLetter).ToString("D2");
                var prefix = $"{branchId}{year:D2}{month:D2}{ascii}";
                var counter = random.Next(1, 99).ToString("D2");
                id = int.Parse($"{prefix}{counter}");
            } while (usedIds.Contains(id));

            usedIds.Add(id);
            return id;
        }

        private static List<TraineeGroup> CreateTraineeGroups(
            Guid tenantId, List<Branch> branches, List<Coach> coaches, Faker faker, Random random)
        {
            var groupNames = new[]
            {
                "Beginners A", "Beginners B", "Intermediate A", "Intermediate B",
                "Advanced A", "Advanced B", "Youth Development", "Junior Stars",
                "Elite Squad", "Weekend Warriors", "Morning Session", "Evening Session"
            };

            return groupNames.Select((name, index) =>
            {
                // Pick the coach first, then bound the group's required skill level by the
                // coach's own - a coach can only lead a group at or below their own level.
                var coach = coaches[random.Next(coaches.Count)];

                // Every third group is private, so a seeded database has both kinds to enroll
                // into. Capacity follows the type rather than one range for all: a private group
                // is small by definition (and the validators cap it at
                // TraineeGroupCapacity.PrivateMaximum), which is what its higher price buys.
                var type = index % 3 == 2 ? TraineeGroupType.Private : TraineeGroupType.Public;

                return new TraineeGroup
                {
                    Name = type == TraineeGroupType.Private ? $"{name} (Private)" : name,
                    Type = type,
                    SkillLevel = (SkillLevel)random.Next(0, (int)coach.SkillLevel + 1),
                    MaximumCapacity = type == TraineeGroupType.Private
                        ? random.Next(3, 7)
                        : random.Next(10, 15),
                    DurationInMinutes = random.Next(2, 4) * 15 + 30,
                    Gender = random.Next(3) switch
                    {
                        0 => TraineeGroupGender.Male,
                        1 => TraineeGroupGender.Female,
                        _ => TraineeGroupGender.Mixed
                    },
                    BranchId = branches[random.Next(branches.Count)].Id,
                    CoachId = coach.EmployeeId,
                    TenantId = tenantId
                };
            }).ToList();
        }

        private static List<GroupSchedule> CreateGroupSchedules(
            Guid tenantId, List<TraineeGroup> groups, Random random)
        {
            var schedules = new List<GroupSchedule>();
            var days = Enum.GetValues<DayOfWeek>().Where(d => d != DayOfWeek.Friday).ToList();

            foreach (var group in groups)
            {
                foreach (var day in days.OrderBy(_ => random.Next()).Take(random.Next(2, 4)))
                {
                    schedules.Add(new GroupSchedule
                    {
                        TraineeGroupId = group.Id,
                        Day = day,
                        StartTime = new TimeOnly(random.Next(8, 20), random.Next(0, 2) * 30),
                        TenantId = tenantId
                    });
                }
            }

            return schedules;
        }

        private static List<PaymentType> CreatePaymentTypes(Guid tenantId)
        {
            return
            [
                new PaymentType { Name = "Cash", IsActive = true, IsDefault = true, TenantId = tenantId },
                new PaymentType { Name = "Online", IsActive = true, IsDefault = false, TenantId = tenantId },
            ];
        }

        private static List<Payment> CreatePayments(
            Guid tenantId, List<Branch> branches, List<PaymentType> paymentTypes, Random random)
        {
            return Enumerable.Range(0, 60).Select(i =>
            {
                return new Payment
                {
                    PaymentNumber = $"PAY-{DateTime.UtcNow.Year}-{random.Next(10000, 99999)}",
                    PaymentTypeId = paymentTypes[random.Next(paymentTypes.Count)].Id,
                    PaidDate = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                    BranchId = branches[random.Next(branches.Count)].Id,
                    Amount = random.Next(20, 80),
                    TenantId = tenantId
                };
            }).ToList();
        }

        // Takes the seeded groups and their schedules because a subscription is no longer
        // independent of them: it records the group type it was priced for and the weekly
        // day-pattern its end date was counted across, and the enrollment picker only offers
        // groups matching both exactly. Inventing a pattern here would produce subscriptions no
        // seeded group can satisfy - they'd look fine in the list and offer nothing to enroll into.
        private static List<SubscriptionDetails> CreateSubscriptionDetails(
            Guid tenantId, List<Trainee> trainees, List<SubscriptionType> subTypes,
            List<SportBranch> sportBranches, List<TraineeGroup> traineeGroups,
            List<GroupSchedule> groupSchedules, Dictionary<int, int> coachSportById,
            List<Payment> payments, Random random)
        {
            var details = new List<SubscriptionDetails>();
            var subscribedTrainees = trainees.Where(_ => random.NextDouble() < 0.7).ToList();

            var daysByGroupId = groupSchedules
                .GroupBy(gs => gs.TraineeGroupId)
                .ToDictionary(g => g.Key, g => g.Select(gs => gs.Day).Distinct().OrderBy(d => d).ToList());

            for (int i = 0; i < subscribedTrainees.Count && i < payments.Count; i++)
            {
                var trainee = subscribedTrainees[i];
                var payment = payments[i];
                var startDate = DateOnly.FromDateTime(payment.PaidDate);

                // Sport and branch come from a real SportBranch pair: the subscription's FK to
                // SportPrice is (Sport, Branch, SubscriptionType, GroupType), so an invented
                // combination wouldn't have a price row to point at.
                var sb = sportBranches[random.Next(sportBranches.Count)];
                var subType = subTypes[random.Next(subTypes.Count)];

                // Copy the type and pattern off a group that actually teaches this sport, so the
                // enrollment step has something to match. Groups pick their branch independently
                // of their coach's sport, so the group's own branch is deliberately ignored here.
                var candidates = traineeGroups
                    .Where(g => coachSportById.TryGetValue(g.CoachId, out var sportId) && sportId == sb.SportId)
                    .Where(g => daysByGroupId.ContainsKey(g.Id))
                    .ToList();

                var groupType = TraineeGroupType.Public;
                var trainingDays = new List<DayOfWeek>();

                if (candidates.Count > 0)
                {
                    var model = candidates[random.Next(candidates.Count)];
                    groupType = model.Type;
                    trainingDays = daysByGroupId[model.Id];
                }

                // Counted across the real pattern, like the application does - not a flat month,
                // which would disagree with the sessions the plan actually grants.
                var endDate = trainingDays.Count > 0
                    ? TrainingScheduleService.ComputeEndDate(
                        startDate,
                        TrainingScheduleService.CalculateTotalSessions(subType.DaysPerMonth, subType.NumberOfMonths),
                        trainingDays)
                    : startDate.AddMonths(subType.NumberOfMonths);

                details.Add(new SubscriptionDetails
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = SubscriptionStatus.Active,
                    TraineeId = trainee.Id,
                    SubscriptionTypeId = subType.Id,
                    SportId = sb.SportId,
                    BranchId = sb.BranchId,
                    GroupType = groupType,
                    TrainingDays = trainingDays,
                    TenantId = tenantId
                });
            }

            return details;
        }

        // One Invoice (with one SubscriptionFee line) per seeded subscription - billing is now
        // a deliberate act separate from payment, so every subscription owes something even
        // before AllocateSeedPayments below decides which of them have actually been paid.
        private static List<Domain.Entities.Finance.Invoice> CreateSeedInvoices(
            Guid tenantId, List<SubscriptionDetails> subscriptionDetails, Random random)
        {
            var invoices = new List<Domain.Entities.Finance.Invoice>();

            for (int i = 0; i < subscriptionDetails.Count; i++)
            {
                var sd = subscriptionDetails[i];
                var price = random.Next(20, 80);

                var invoice = new Domain.Entities.Finance.Invoice
                {
                    InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{i + 1:D5}",
                    Status = InvoiceStatus.Issued,
                    IssueDate = sd.StartDate,
                    DueDate = sd.StartDate.AddDays(7),
                    TraineeId = sd.TraineeId,
                    BranchId = sd.BranchId,
                    Currency = "KWD",
                    SubTotal = price,
                    GrandTotal = price,
                    AmountPaid = 0,
                    TenantId = tenantId,
                };
                invoice.Lines.Add(new Domain.Entities.Finance.InvoiceLine
                {
                    Type = InvoiceLineType.SubscriptionFee,
                    Description = "Subscription fee",
                    Quantity = 1,
                    UnitPrice = price,
                    LineTotal = price,
                    SubscriptionDetailsId = sd.Id,
                });

                invoices.Add(invoice);
            }

            return invoices;
        }

        // Allocates ~70% of the seeded payments against their paired invoice (mirroring
        // CreateSubscriptionDetails' index-based payment pairing), leaving the rest unpaid so
        // the Accountant console's outstanding/overdue views have real data to show.
        private static List<Domain.Entities.Finance.PaymentAllocation> AllocateSeedPayments(
            List<SubscriptionDetails> subscriptionDetails,
            List<Domain.Entities.Finance.Invoice> invoices,
            List<Payment> payments,
            Random random)
        {
            var allocations = new List<Domain.Entities.Finance.PaymentAllocation>();

            for (int i = 0; i < subscriptionDetails.Count && i < payments.Count; i++)
            {
                if (random.NextDouble() >= 0.7) continue;

                var invoice = invoices[i];
                var payment = payments[i];
                var amount = Math.Min(payment.Amount, invoice.GrandTotal);

                allocations.Add(new Domain.Entities.Finance.PaymentAllocation
                {
                    PaymentNumber = payment.PaymentNumber,
                    InvoiceId = invoice.Id,
                    Amount = amount,
                });

                invoice.AmountPaid += amount;
                invoice.Status = invoice.AmountPaid >= invoice.GrandTotal
                    ? InvoiceStatus.Paid
                    : InvoiceStatus.PartiallyPaid;
            }

            return allocations;
        }

        // Also derives the SportTrainee skill record each enrollment implies - none were
        // seeded before, so there's no prior truth to preserve; a group is picked to satisfy
        // the app's own placement rules (same sport, compatible gender, and skill at-or-below
        // whatever this trainee is recorded at for that sport so far), and the recorded skill
        // is raised to match if no group at their current level is available.
        private static (List<Enrollment> Enrollments, List<SportTrainee> SportTrainees) CreateEnrollments(
            Guid tenantId, List<Trainee> trainees, List<TraineeGroup> groups, List<Coach> coaches,
            List<SubscriptionType> subTypes, List<SubscriptionDetails> subscriptionDetails, Random random)
        {
            var coachSportById = coaches.ToDictionary(c => c.EmployeeId, c => c.SportId);
            var sessionsBySubscriptionTypeId = subTypes.ToDictionary(
                st => st.Id,
                st => TrainingScheduleService.CalculateTotalSessions(st.DaysPerMonth, st.NumberOfMonths));
            var traineeById = trainees.ToDictionary(t => t.Id);
            var skillByTraineeSport = new Dictionary<(int TraineeId, int SportId), SkillLevel>();
            var enrollments = new List<Enrollment>();

            foreach (var sd in subscriptionDetails)
            {
                var trainee = traineeById[sd.TraineeId];

                // Mandatory: same sport as the subscription (the app itself rejects a
                // mismatch - see SubscriptionGroupSportMismatchException) and a compatible
                // gender (Mixed accepts anyone, Male/Female groups don't accept the other).
                var candidates = groups
                    .Where(g => coachSportById[g.CoachId] == sd.SportId)
                    .Where(g => g.Gender == TraineeGroupGender.Mixed
                        || (g.Gender == TraineeGroupGender.Male) == (trainee.Gender == Gender.Male))
                    // Priced for this kind of group - CreateEnrollmentCommandHandler throws
                    // SubscriptionGroupTypeMismatchException on a mismatch, so seeding one would
                    // produce demo data the application itself would refuse to create.
                    .Where(g => g.Type == sd.GroupType)
                    .ToList();

                if (candidates.Count == 0)
                    continue; // no compatible group seeded for this sport/gender - leave the subscription unclaimed, same as a real not-yet-enrolled trainee

                var key = (sd.TraineeId, sd.SportId);
                var recordedSkill = skillByTraineeSport.TryGetValue(key, out var s) ? s : (SkillLevel?)null;

                var fitting = recordedSkill.HasValue
                    ? candidates.Where(g => g.SkillLevel <= recordedSkill.Value).ToList()
                    : candidates;

                var group = fitting.Count > 0
                    ? fitting[random.Next(fitting.Count)]
                    : candidates[random.Next(candidates.Count)];

                skillByTraineeSport[key] = recordedSkill.HasValue && recordedSkill.Value > group.SkillLevel
                    ? recordedSkill.Value
                    : group.SkillLevel;

                // The plan's own quota for the whole term, as CreateEnrollmentCommandHandler
                // assigns it - not a flat 8, which bore no relation to what the trainee bought
                // and left Gold and Quarterly subscribers looking short-changed in the demo data.
                var sessionsAllowed = sessionsBySubscriptionTypeId.GetValueOrDefault(sd.SubscriptionTypeId, 8);

                enrollments.Add(new Enrollment
                {
                    EnrollmentDate = sd.StartDate.ToDateTime(TimeOnly.MinValue),
                    ExpiryDate = sd.EndDate.ToDateTime(TimeOnly.MinValue),
                    SessionAllowed = sessionsAllowed,
                    SessionRemaining = random.Next(0, sessionsAllowed + 1),
                    IsActive = true,
                    TraineeId = sd.TraineeId,
                    TraineeGroupId = group.Id,
                    SubscriptionDetailsId = sd.Id,
                    TenantId = tenantId
                });
            }

            var sportTrainees = skillByTraineeSport.Select(kv => new SportTrainee
            {
                TraineeId = kv.Key.TraineeId,
                SportId = kv.Key.SportId,
                SkillLevel = kv.Value,
                TenantId = tenantId
            }).ToList();

            return (enrollments, sportTrainees);
        }

        private static string GenerateKuwaitiSSN(Random random, int minYear, int maxYear)
        {
            var year = random.Next(minYear, maxYear + 1) % 100;
            var month = random.Next(1, 13);
            var day = random.Next(1, 29);
            return $"{year:D2}{month:D2}{day:D2}{random.Next(100000, 999999)}";
        }

        private static string GenerateKuwaitiPhone(Random random)
        {
            var prefixes = new[] { 5, 6, 9 };
            return $"{prefixes[random.Next(prefixes.Length)]}{random.Next(1000000, 9999999)}";
        }
    }
}
