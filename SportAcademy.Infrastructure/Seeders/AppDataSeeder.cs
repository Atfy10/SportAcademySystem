using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Seeders
{
    public class AppDataSeeder
    {
        private const string DefaultPassword = "Admin@123";

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

        public AppDataSeeder(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ILogger<AppDataSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            if (await _context.Tenants.IgnoreQueryFilters().AnyAsync())
            {
                _logger.LogInformation("Database already seeded. Skipping.");
                return;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("=== Starting Database Seeding ===");

                await DisableUserTenantFkAsync();

                var systemTenantId = Guid.NewGuid();
                var superAdminId = Guid.NewGuid();
                var salmiyaTenantId = Guid.NewGuid();
                var ownerId = Guid.NewGuid();

                await SeedUsersAsync(systemTenantId, superAdminId, salmiyaTenantId, ownerId);
                await SeedTenantsAsync(systemTenantId, superAdminId, salmiyaTenantId, ownerId);

                await EnableUserTenantFkAsync();

                await SeedRolesAsync(systemTenantId, salmiyaTenantId);
                await AssignRolesAsync(superAdminId, ownerId, salmiyaTenantId);

                var featureIds = await SeedFeaturesAsync();
                var enterprisePlanIds = await SeedSubscriptionPlansAsync(featureIds);

                var natCatIds = await SeedNationalityCategoriesAsync();
                await SeedTenantSettingsAsync(salmiyaTenantId, enterprisePlanIds);
                await EnableTenantFeaturesAsync(salmiyaTenantId, featureIds);

                await SeedSalmiyaDataAsync(salmiyaTenantId, natCatIds);

                await transaction.CommitAsync();
                _logger.LogInformation("=== Database Seeding Completed Successfully ===");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database seeding failed. Transaction rolled back.");
                throw;
            }
        }

        private async Task DisableUserTenantFkAsync()
        {
            _logger.LogDebug("Disabling circular FK constraints for seeding...");
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE [AspNetUsers] NOCHECK CONSTRAINT [FK_AspNetUsers_Tenants_TenantId]");
        }

        private async Task EnableUserTenantFkAsync()
        {
            _logger.LogDebug("Re-enabling FK constraints...");
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE [AspNetUsers] WITH CHECK CHECK CONSTRAINT [FK_AspNetUsers_Tenants_TenantId]");
        }

        private async Task SeedUsersAsync(
            Guid systemTenantId, Guid superAdminId,
            Guid salmiyaTenantId, Guid ownerId)
        {
            _logger.LogInformation("Seeding users...");

            var superAdmin = new AppUser
            {
                Id = superAdminId,
                UserName = "superadmin",
                Email = "abdulrahmannalatfy@gmail.com",
                TenantId = systemTenantId,
                IsPasswordReset = false,
                IsBanned = false,
                EmailConfirmed = true,
                PhoneNumber = "+201096042061",
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = true
            };
            var result = await _userManager.CreateAsync(superAdmin, DefaultPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create SuperAdmin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            _context.Profiles.Add(new Profile { AppUserId = superAdmin.Id });

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
            result = await _userManager.CreateAsync(owner, DefaultPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create Owner: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            _context.Profiles.Add(new Profile { AppUserId = owner.Id });

            var adminUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin.salmiya",
                Email = "admin@salmiya-academy.com.kw",
                TenantId = salmiyaTenantId,
                IsPasswordReset = false,
                IsBanned = false,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true
            };
            result = await _userManager.CreateAsync(adminUser, DefaultPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create Admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            _context.Profiles.Add(new Profile { AppUserId = adminUser.Id });

            var managerUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "manager.salmiya",
                Email = "manager@salmiya-academy.com.kw",
                TenantId = salmiyaTenantId,
                IsPasswordReset = false,
                IsBanned = false,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = true
            };
            result = await _userManager.CreateAsync(managerUser, DefaultPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create Manager: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            _context.Profiles.Add(new Profile { AppUserId = managerUser.Id });

            var accountantUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "accountant.salmiya",
                Email = "accountant@salmiya-academy.com.kw",
                TenantId = salmiyaTenantId,
                IsPasswordReset = false,
                IsBanned = false,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = true
            };
            result = await _userManager.CreateAsync(accountantUser, DefaultPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create Accountant: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            _context.Profiles.Add(new Profile { AppUserId = accountantUser.Id });

            var coachEmails = new[] { "ahmed.ali", "khaled.omar", "sultan.hamad", "fahad.naser", "nawaf.mutairi" };
            for (int i = 0; i < coachEmails.Length; i++)
            {
                var coachUser = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = coachEmails[i],
                    Email = $"{coachEmails[i]}@salmiya-academy.com.kw",
                    TenantId = salmiyaTenantId,
                    IsPasswordReset = false,
                    IsBanned = false,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true
                };
                result = await _userManager.CreateAsync(coachUser, DefaultPassword);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Failed to create Coach {coachEmails[i]}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                _context.Profiles.Add(new Profile { AppUserId = coachUser.Id });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Users seeded successfully.");
        }

        private async Task SeedTenantsAsync(
            Guid systemTenantId, Guid superAdminId,
            Guid salmiyaTenantId, Guid ownerId)
        {
            _logger.LogInformation("Seeding tenants...");

            var now = DateTime.UtcNow;

            var systemTenant = new Tenant
            {
                Id = systemTenantId,
                Name = "System",
                DisplayName = "System Platform",
                Email = "system@sportacademy.com.kw",
                Code = "SYSTEM",
                Slug = "system",
                Status = TenantStatus.Active,
                OwnerId = superAdminId,
                CreatedAt = now
            };
            _context.Tenants.Add(systemTenant);

            var salmiyaTenant = new Tenant
            {
                Id = salmiyaTenantId,
                Name = "Salmiya Academy",
                DisplayName = "Salmiya Swimming Academy",
                Email = "info@salmiya-academy.com.kw",
                Code = "SALMYIA",
                Slug = "salmiya-academy",
                Status = TenantStatus.Active,
                OwnerId = ownerId,
                CreatedAt = now
            };
            _context.Tenants.Add(salmiyaTenant);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Tenants seeded successfully.");
        }

        private async Task SeedRolesAsync(Guid systemTenantId, Guid salmiyaTenantId)
        {
            _logger.LogInformation("Seeding roles...");

            if (!await _roleManager.RoleExistsAsync("SuperAdmin"))
            {
                var superAdminRole = new AppRole { Name = "SuperAdmin", TenantId = systemTenantId };
                var result = await _roleManager.CreateAsync(superAdminRole);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Failed to create SuperAdmin role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var roleNames = new[] { "Owner", "Admin", "Manager", "User", "Coach", "Accountant" };
            foreach (var roleName in roleNames)
            {
                var exists = await _roleManager.RoleExistsAsync(roleName);
                if (!exists)
                {
                    var role = new AppRole
                    {
                        Name = roleName,
                        TenantId = salmiyaTenantId
                    };
                    var result = await _roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                        throw new InvalidOperationException($"Failed to create role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            _logger.LogInformation("Roles seeded successfully.");
        }

        private async Task AssignRolesAsync(Guid superAdminId, Guid ownerId, Guid salmiyaTenantId)
        {
            _logger.LogInformation("Assigning roles to users...");

            var superAdmin = await _userManager.FindByIdAsync(superAdminId.ToString());
            if (superAdmin != null)
                await _userManager.AddToRoleAsync(superAdmin, "SuperAdmin");

            var owner = await _userManager.FindByIdAsync(ownerId.ToString());
            if (owner != null)
                await _userManager.AddToRoleAsync(owner, "Owner");

            var admin = await _userManager.FindByEmailAsync("admin@salmiya-academy.com.kw");
            if (admin != null)
                await _userManager.AddToRoleAsync(admin, "Admin");

            var manager = await _userManager.FindByEmailAsync("manager@salmiya-academy.com.kw");
            if (manager != null)
                await _userManager.AddToRoleAsync(manager, "Manager");

            var accountant = await _userManager.FindByEmailAsync("accountant@salmiya-academy.com.kw");
            if (accountant != null)
                await _userManager.AddToRoleAsync(accountant, "Accountant");

            var coachEmails = new[] { "ahmed.ali", "khaled.omar", "sultan.hamad", "fahad.naser", "nawaf.mutairi" };
            foreach (var email in coachEmails)
            {
                var coachUser = await _userManager.FindByEmailAsync($"{email}@salmiya-academy.com.kw");
                if (coachUser != null)
                    await _userManager.AddToRoleAsync(coachUser, "Coach");
            }

            _logger.LogInformation("Roles assigned successfully.");
        }

        private async Task<List<Guid>> SeedFeaturesAsync()
        {
            _logger.LogInformation("Seeding system features...");

            var features = new List<Feature>
            {
                CreateFeature("user-management", "User Management", "Create, edit, and manage system users"),
                CreateFeature("role-management", "Role & Permission Management", "Define roles and assign permissions"),
                CreateFeature("tenant-settings", "Tenant Configuration", "Configure tenant-wide settings"),
                CreateFeature("branch-management", "Branch Management", "Manage academy branches and locations"),
                CreateFeature("trainee-management", "Trainee Management", "Register and manage trainee profiles"),
                CreateFeature("employee-management", "Employee Management", "Manage staff and employee records"),
                CreateFeature("coach-management", "Coach Management", "Assign and manage coaches"),
                CreateFeature("sport-management", "Sports Management", "Define sports and training activities"),
                CreateFeature("subscription-plan", "Subscription Plans", "Create and manage subscription offerings"),
                CreateFeature("pricing-management", "Pricing Management", "Set sport and branch pricing"),
                CreateFeature("payment-processing", "Payment Processing", "Process and track payments"),
                CreateFeature("group-management", "Group Management", "Form and manage training groups"),
                CreateFeature("schedule-management", "Schedule Management", "Create class schedules and timetables"),
                CreateFeature("attendance-tracking", "Attendance Tracking", "Record and report attendance"),
                CreateFeature("enrollment-management", "Enrollment Management", "Manage trainee enrollments"),
                CreateFeature("family-management", "Family Management", "Manage family accounts and billing"),
                CreateFeature("nationality-categories", "Nationality Categories", "Configure nationality classifications"),
                CreateFeature("financial-reports", "Financial Reports", "Revenue and payment analytics"),
                CreateFeature("trainee-reports", "Trainee Reports", "Progress and performance reports"),
                CreateFeature("coach-reports", "Coach Reports", "Coach evaluation and analytics"),
                CreateFeature("operational-reports", "Operational Reports", "Daily operations reporting"),
                CreateFeature("attendance-reports", "Attendance Reports", "Attendance analytics and insights"),
                CreateFeature("notifications", "Notification System", "Send and manage system notifications"),
                CreateFeature("chat-system", "In-App Chat", "Internal messaging and communication"),
                CreateFeature("video-analysis", "AI Video Analysis", "AI-powered sports video analysis"),
                CreateFeature("health-test-mgmt", "Health Test Management", "Track health assessments and tests"),
                CreateFeature("discount-offers", "Discounts & Offers", "Manage promotions and discounts"),
                CreateFeature("session-management", "Session Management", "Manage training sessions"),
                CreateFeature("audit-trail", "Audit Trail", "System activity logging"),
                CreateFeature("system-settings", "System Settings", "Global system configuration"),
                CreateFeature("profile-mgmt", "Profile Management", "User profile and preferences"),
                CreateFeature("ai-assistant", "AI Assistant", "AI-powered help and insights"),
                CreateFeature("api-access", "API Access", "External API integration management"),
                CreateFeature("backup-restore", "Backup & Restore", "Data backup and restoration"),
                CreateFeature("trainee-codes", "Trainee Code Management", "Custom trainee code assignment")
            };

            _context.Set<Feature>().AddRange(features);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Features seeded successfully.");
            return features.Select(f => f.Id).ToList();
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

        private async Task<int> SeedSubscriptionPlansAsync(List<Guid> featureIds)
        {
            _logger.LogInformation("Seeding subscription plans...");

            var plans = new List<SubscriptionPlan>
            {
                new() { Name = "Basic", Code = "BASIC", Description = "Core features for small academies", MonthlyPrice = 49, YearlyPrice = 499, IsActive = true },
                new() { Name = "Professional", Code = "PRO", Description = "Full feature set for growing academies", MonthlyPrice = 99, YearlyPrice = 999, IsActive = true },
                new() { Name = "Enterprise", Code = "ENTERPRISE", Description = "Complete suite with AI and advanced analytics", MonthlyPrice = 199, YearlyPrice = 1999, IsActive = true }
            };

            _context.SubscriptionPlans.AddRange(plans);
            await _context.SaveChangesAsync();

            var basicFeatures = featureIds.Take(15).ToList();
            var professionalFeatures = featureIds.Take(28).ToList();
            var enterpriseFeatures = featureIds.ToList();

            foreach (var plan in plans)
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
            _logger.LogInformation("Subscription plans seeded successfully.");
            return plans.Where(p => p.Code == "ENTERPRISE").Select(p => p.Id).First();
        }

        private async Task<Dictionary<string, int>> SeedNationalityCategoriesAsync()
        {
            _logger.LogInformation("Seeding nationality categories...");

            var categories = new List<NationalityCategory>
            {
                new() { Code = "KW", Name = "Kuwaiti" },
                new() { Code = "GCC", Name = "GCC National" },
                new() { Code = "AR", Name = "Arab (Non-GCC)" },
                new() { Code = "AS", Name = "Asian" },
                new() { Code = "AF", Name = "African" },
                new() { Code = "EU", Name = "European" },
                new() { Code = "NA", Name = "North American" },
                new() { Code = "SA", Name = "South American" },
                new() { Code = "OT", Name = "Other" }
            };

            _context.NationalityCategories.AddRange(categories);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nationality categories seeded successfully.");
            return categories.ToDictionary(c => c.Code, c => c.Id);
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

            var sportBranches = CreateSportBranches(sports, branches);
            _context.Set<SportBranch>().AddRange(sportBranches);
            await _context.SaveChangesAsync();

            var sportSubTypes = CreateSportSubscriptionTypes(sports, subTypes);
            _context.Set<SportSubscriptionType>().AddRange(sportSubTypes);
            await _context.SaveChangesAsync();

            var basePrices = new Dictionary<string, decimal>
            {
                ["Swimming"] = 60m, ["Football"] = 45m, ["Basketball"] = 40m,
                ["Volleyball"] = 35m, ["Tennis"] = 55m, ["Martial Arts"] = 50m,
                ["Gymnastics"] = 65m, ["Table Tennis"] = 30m
            };
            var sportPriceLookup = sports.ToDictionary(s => s.Id, s => basePrices.GetValueOrDefault(s.Name, 40m));
            var sportPrices = CreateSportPrices(sportBranches, sportPriceLookup, subTypes);
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

            var payments = CreatePayments(tenantId, branches, random);
            _context.Set<Payment>().AddRange(payments);
            await _context.SaveChangesAsync();

            var subscriptionDetails = CreateSubscriptionDetails(tenantId, trainees, subTypes, sportBranches, payments, random);
            _context.Set<SubscriptionDetails>().AddRange(subscriptionDetails);
            await _context.SaveChangesAsync();

            var enrollments = CreateEnrollments(tenantId, trainees, traineeGroups, subscriptionDetails, random);
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
                new() { Name = SubType.Monthly, DaysPerMonth = 8, NumberOfMonths = 1, IsActive = true, IsOffer = false, TenantId = tenantId },
                new() { Name = SubType.Quarterly, DaysPerMonth = 10, NumberOfMonths = 3, IsActive = true, IsOffer = false, TenantId = tenantId },
                new() { Name = SubType.Silver, DaysPerMonth = 12, NumberOfMonths = 1, IsActive = true, IsOffer = false, TenantId = tenantId },
                new() { Name = SubType.Gold, DaysPerMonth = 16, NumberOfMonths = 1, IsActive = true, IsOffer = false, TenantId = tenantId },
                new() { Name = SubType.Platinum, DaysPerMonth = 24, NumberOfMonths = 1, IsActive = true, IsOffer = true, TenantId = tenantId }
            };
        }

        private static List<SportBranch> CreateSportBranches(List<Sport> sports, List<Branch> branches)
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
                        IsAvailable = true
                    });
                }
            }

            return result;
        }

        private static List<SportSubscriptionType> CreateSportSubscriptionTypes(List<Sport> sports, List<SubscriptionType> subTypes)
        {
            return sports.SelectMany(sport =>
                subTypes.Select(st => new SportSubscriptionType
                {
                    SportId = sport.Id,
                    SubscriptionTypeId = st.Id
                })
            ).ToList();
        }

        private static List<SportPrice> CreateSportPrices(
            List<SportBranch> sportBranches, Dictionary<int, decimal> sportPriceLookup,
            List<SubscriptionType> subTypes)
        {
            var priceMultipliers = new Dictionary<SubType, decimal>
            {
                [SubType.Monthly] = 1.0m, [SubType.Quarterly] = 2.8m,
                [SubType.Silver] = 1.4m, [SubType.Gold] = 1.8m, [SubType.Platinum] = 2.5m
            };

            var random = new Random();
            var result = new List<SportPrice>();

            foreach (var sb in sportBranches)
            {
                var basePrice = sportPriceLookup.GetValueOrDefault(sb.SportId, 40m);
                foreach (var st in subTypes)
                {
                    var multiplier = priceMultipliers.GetValueOrDefault(st.Name, 1.0m);
                    result.Add(new SportPrice
                    {
                        SportId = sb.SportId,
                        BranchId = sb.BranchId,
                        SubsTypeId = st.Id,
                        Price = Math.Round(basePrice * multiplier + random.Next(-5, 5), 2)
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
                    BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-random.Next(22, 55))),
                    Gender = emp.Gender,
                    Nationality = emp.Gender == Gender.Female && emp.Last.StartsWith("Al-")
                        ? Nationality.Kuwaiti
                        : Nationality.Kuwaiti,
                    PhoneNumber = GenerateKuwaitiPhone(random),
                    SecondPhoneNumber = random.NextDouble() < 0.3 ? GenerateKuwaitiPhone(random) : null,
                    Address = Address.Create($"Street {random.Next(1, 250)}, Block {random.Next(1, 12)}", branch.City),
                    Email = Email.Create($"{emp.First.ToLower()}.{emp.Last.ToLower()}@salmiya-academy.com.kw"),
                    Salary = random.Next(400, 1500),
                    HireDate = DateTime.Now.AddDays(-random.Next(30, 1095)),
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
                var birthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-random.Next(6, 35)));

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
                    JoinDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-random.Next(30, 730))),
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

            return groupNames.Select(name => new TraineeGroup
            {
                Name = name,
                SkillLevel = (SkillLevel)random.Next(0, 4),
                MaximumCapacity = random.Next(10, 15),
                DurationInMinutes = random.Next(2, 4) * 15 + 30,
                Gender = random.Next(2) == 0 ? Gender.Male : Gender.Female,
                BranchId = branches[random.Next(branches.Count)].Id,
                CoachId = coaches[random.Next(coaches.Count)].EmployeeId,
                TenantId = tenantId
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

        private static List<Payment> CreatePayments(
            Guid tenantId, List<Branch> branches, Random random)
        {
            return Enumerable.Range(0, 60).Select(i =>
            {
                return new Payment
                {
                    PaymentNumber = $"PAY-{DateTime.UtcNow.Year}-{random.Next(10000, 99999)}",
                    Method = random.Next(2) == 0 ? PaymentMethod.Cash : PaymentMethod.Online,
                    PaidDate = DateTime.Now.AddDays(-random.Next(1, 180)),
                    BranchId = branches[random.Next(branches.Count)].Id,
                    TenantId = tenantId
                };
            }).ToList();
        }

        private static List<SubscriptionDetails> CreateSubscriptionDetails(
            Guid tenantId, List<Trainee> trainees, List<SubscriptionType> subTypes,
            List<SportBranch> sportBranches,
            List<Payment> payments, Random random)
        {
            var details = new List<SubscriptionDetails>();
            var subscribedTrainees = trainees.Where(_ => random.NextDouble() < 0.7).ToList();

            for (int i = 0; i < subscribedTrainees.Count && i < payments.Count; i++)
            {
                var trainee = subscribedTrainees[i];
                var payment = payments[i];
                var startDate = DateOnly.FromDateTime(payment.PaidDate);
                var sb = sportBranches[random.Next(sportBranches.Count)];

                details.Add(new SubscriptionDetails
                {
                    StartDate = startDate,
                    EndDate = startDate.AddMonths(1),
                    Status = SubscriptionStatus.Active,
                    PaymentNumber = payment.PaymentNumber,
                    TraineeId = trainee.Id,
                    SubscriptionTypeId = subTypes[random.Next(subTypes.Count)].Id,
                    SportId = sb.SportId,
                    BranchId = sb.BranchId,
                    TenantId = tenantId
                });
            }

            return details;
        }

        private static List<Enrollment> CreateEnrollments(
            Guid tenantId, List<Trainee> trainees, List<TraineeGroup> groups,
            List<SubscriptionDetails> subscriptionDetails, Random random)
        {
            return subscriptionDetails.Select(sd =>
            {
                var group = groups[random.Next(groups.Count)];
                return new Enrollment
                {
                    EnrollmentDate = sd.StartDate.ToDateTime(TimeOnly.MinValue),
                    ExpiryDate = sd.EndDate.ToDateTime(TimeOnly.MinValue),
                    SessionAllowed = 8,
                    SessionRemaining = random.Next(0, 9),
                    IsActive = true,
                    TraineeId = sd.TraineeId,
                    TraineeGroupId = group.Id,
                    SubscriptionDetailsId = sd.Id,
                    TenantId = tenantId
                };
            }).ToList();
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
