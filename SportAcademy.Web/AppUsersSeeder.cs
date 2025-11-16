using Microsoft.AspNetCore.Identity;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Web
{
    public class AspUsersSeeder
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AspUsersSeeder> _logger;

        public AspUsersSeeder(UserManager<AppUser> userManager, ILogger<AspUsersSeeder> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedUsersAsync()
        {
            try
            {
                _logger.LogInformation("Starting user seeding process...");

                // Check if users already exist to avoid duplicates
                var existingUsersCount = _userManager.Users.Count();
                if (existingUsersCount >= 100)
                {
                    _logger.LogInformation($"Users already seeded. Current count: {existingUsersCount}");
                    return;
                }

                var random = new Random();
                var usersToCreate = 100 - existingUsersCount;

                for (int i = 1; i <= usersToCreate; i++)
                {
                    var user = new AppUser
                    {
                        UserName = $"user{(i + 50):D3}@example.com",
                        Email = $"user{(i + 50):D3}@example.com",
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true,
                        TwoFactorEnabled = false,
                        LockoutEnabled = true,
                        AccessFailedCount = 0,
                        IsBanned = random.Next(0, 10) == 0 // 10% chance of being banned
                    };

                    // Create user with default password
                    var result = await _userManager.CreateAsync(user, "TempPassword123!");

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Successfully created user: {user.UserName}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to create user {user.UserName}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                _logger.LogInformation($"User seeding completed. Total users in database: {_userManager.Users.Count()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user seeding");
                throw;
            }
        }

        // Alternative method with more realistic data
        public async Task SeedUsersWithRealisticDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting realistic user seeding process...");

                var existingUsersCount = _userManager.Users.Count();
                if (existingUsersCount >= 100)
                {
                    _logger.LogInformation($"Users already seeded. Current count: {existingUsersCount}");
                    return;
                }

                var firstNames = new List<string>
            {
                "John", "Jane", "Michael", "Sarah", "David", "Lisa", "Robert", "Emily",
                "James", "Ashley", "William", "Jessica", "Richard", "Amanda", "Thomas",
                "Jennifer", "Charles", "Michelle", "Christopher", "Melissa", "Daniel",
                "Kimberly", "Matthew", "Donna", "Anthony", "Carol", "Mark", "Ruth",
                "Donald", "Sharon", "Steven", "Laura", "Paul", "Sandra", "Andrew",
                "Cynthia", "Kenneth", "Kathleen", "Joshua", "Amy", "Kevin", "Angela",
                "Brian", "Helen", "George", "Deborah", "Timothy", "Rachel", "Ronald", "Carolyn"
            };

                var lastNames = new List<string>
            {
                "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller",
                "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez",
                "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
                "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark",
                "Ramirez", "Lewis", "Robinson", "Walker", "Young", "Allen", "King",
                "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green",
                "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts"
            };

                var random = new Random();
                var usersToCreate = 100 - existingUsersCount;

                for (int i = 0; i < usersToCreate; i++)
                {
                    var firstName = firstNames[random.Next(firstNames.Count)];
                    var lastName = lastNames[random.Next(lastNames.Count)];
                    var username = $"{firstName.ToLower()}.{lastName.ToLower()}{random.Next(100, 999)}";
                    var email = $"{username}@example.com";

                    var user = new AppUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = random.Next(0, 10) > 1, // 90% confirmed
                        PhoneNumber = GeneratePhoneNumber(random),
                        PhoneNumberConfirmed = random.Next(0, 10) > 3, // 70% confirmed
                        TwoFactorEnabled = random.Next(0, 10) > 7, // 30% enabled
                        LockoutEnabled = true,
                        AccessFailedCount = random.Next(0, 3),
                        IsBanned = random.Next(0, 20) == 0 // 5% chance of being banned
                    };

                    var result = await _userManager.CreateAsync(user, "DefaultPassword123!");

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Successfully created user: {user.UserName}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to create user {user.UserName}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                _logger.LogInformation($"Realistic user seeding completed. Total users: {_userManager.Users.Count()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during realistic user seeding");
                throw;
            }
        }

        private string GeneratePhoneNumber(Random random)
        {
            // Generate a Kuwait phone number format: XXXX XXXX
            // Kuwait mobile numbers start with 5, 6, 9 for mobiles
            // Landline numbers start with 2 for Kuwait City area

            var prefixes = new[] { 5, 6, 9 }; // Mobile prefixes in Kuwait
            var selectedPrefix = prefixes[random.Next(prefixes.Length)];

            // Generate 7 more digits after the prefix
            var remainingDigits = random.Next(1000000, 9999999);

            return $"{selectedPrefix}{remainingDigits:D7}";
        }
    }
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserSeeder(this IServiceCollection services)
        {
            services.AddScoped<AspUsersSeeder>();
            return services;
        }
    }
    public static class DatabaseInitializer
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<AspUsersSeeder>();

            // Use either method based on your preference
            await seeder.SeedUsersAsync(); // Simple seeding
                                           // OR
                                           // await seeder.SeedUsersWithRealisticDataAsync(); // More realistic data
        }
    }
}
