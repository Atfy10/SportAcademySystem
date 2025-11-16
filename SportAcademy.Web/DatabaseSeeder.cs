using Bogus;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;
using SportAcademy.Infrastructure.DBContext;

namespace SportAcademy.Infrastructure.Seeders
{
    public static class DatabaseSeeder
    {
        private static readonly List<string> KuwaitiAreas = new()
        {
            "Salmiya", "Hawally", "Jabriya", "Fintas", "Mahboula",
            "Mangaf", "Fahaheel", "Ahmadi", "Farwaniya", "Jleeb Al-Shuyoukh",
            "Sabah Al-Salem", "Rumaithiya", "Bayan", "Mishref", "Salwa",
            "Abdullah Al-Mubarak", "Jaber Al-Ahmad", "South Surra", "Kaifan", "Sharq",
            "Sabah Al-Nasser", "Abu Halifa", "Al-Qurain", "Qortuba", "Dasma"
        };

        private static readonly List<string> KuwaitiStreets = new()
        {
            "Gulf Road", "Arabian Gulf Street", "Salem Al-Mubarak Street",
            "Baghdad Street", "Tunis Street", "Damascus Street",
            "Beirut Street", "Mecca Street", "Abdullah Al-Mubarak Street",
            "Fahaheel Expressway", "King Fahd Road", "Jamal Abdul Nasser Street",
            "First Ring Road", "Second Ring Road", "Third Ring Road",
            "Fourth Ring Road", "Fifth Ring Road", "Sixth Ring Road",
            "Coastal Road", "Airport Road"
        };

        private static readonly List<string> KuwaitiFirstNames = new()
        {
            // Male Names
            "Mohammed", "Ahmed", "Abdullah", "Fahad", "Khalid", "Sultan", "Faisal", "Hamad",
            "Meshal", "Nawaf", "Talal", "Jassim", "Yousef", "Ali", "Hassan", "Omar",
            "Bader", "Nasser", "Saud", "Majed", "Nayef", "Thamer", "Bandar", "Rashid",
            // Female Names
            "Fatima", "Maryam", "Aisha", "Noura", "Sarah", "Layla", "Hessa", "Shaikha",
            "Latifa", "Moza", "Amna", "Haya", "Dana", "Reem", "Lulwa", "Anoud"
        };

        private static readonly List<string> EgyptianFirstNames = new()
        {
            // Male Names
            "Mohamed", "Ahmed", "Mahmoud", "Mostafa", "Youssef", "Karim", "Omar", "Hassan",
            "Ali", "Amr", "Khaled", "Tarek", "Sherif", "Hossam", "Magdy", "Samir",
            "Adel", "Essam", "Ashraf", "Wael",
            // Female Names
            "Fatma", "Mariam", "Nour", "Yasmin", "Salma", "Heba", "Dina", "Rana",
            "Noha", "Hala", "Eman", "Shaimaa", "Amal", "Rania", "Nada", "Mona"
        };

        private static readonly List<string> ArabicLastNames = new()
        {
            "Al-Mutairi", "Al-Ajmi", "Al-Rashidi", "Al-Dosari", "Al-Harbi",
            "Al-Shammari", "Al-Otaibi", "Al-Anzi", "Al-Qahtani", "Al-Ghanim",
            "Al-Sabah", "Al-Salem", "Al-Ahmad", "Al-Ali", "Al-Khaled",
            "Mohamed", "Ibrahim", "Hassan", "Hussein", "Abdullah",
            "Ismail", "Mansour", "Farouk", "Sayed", "Othman", "Abdel Rahman"
        };

        public static async Task SeedDatabase(ApplicationDbContext context, ILogger logger)
        {
            logger.LogInformation("Starting database seeding...");

            if (context.Branchs.Any())
            {
                logger.LogInformation("Database already seeded. Skipping seeding process.");
                return;
            }

            // 1. Branches
            logger.LogInformation("1️⃣ Seeding Branches...");
            var branches = GenerateBranches();
            await context.Branchs.AddRangeAsync(branches);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ Branches seeded successfully.");

            // 2. Sports
            logger.LogInformation("2️⃣ Seeding Sports...");
            var sports = GenerateSports();
            await context.Sports.AddRangeAsync(sports);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ Sports seeded successfully.");

            // 3. Subscription Types
            logger.LogInformation("3️⃣ Seeding SubscriptionTypes...");
            var subscriptionTypes = GenerateSubscriptionTypes();
            await context.SubscriptionTypes.AddRangeAsync(subscriptionTypes);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ SubscriptionTypes seeded successfully.");

            // 4. SportBranch
            logger.LogInformation("4️⃣ Seeding SportBranches...");
            var sportBranches = GenerateSportBranches(sports, branches);
            await context.Set<SportBranch>().AddRangeAsync(sportBranches);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ SportBranches seeded successfully.");

            // 5. SportSubscriptionType
            logger.LogInformation("5️⃣ Seeding SportSubscriptionTypes...");
            var sportSubscriptionTypes = GenerateSportSubscriptionTypes(sports, subscriptionTypes);
            await context.Set<SportSubscriptionType>().AddRangeAsync(sportSubscriptionTypes);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ SportSubscriptionTypes seeded successfully.");

            // 6. SportPrices
            logger.LogInformation("6️⃣ Seeding SportPrices...");
            var sportPrices = GenerateSportPrices(sports, branches, subscriptionTypes);
            await context.Set<SportPrice>().AddRangeAsync(sportPrices);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ SportPrices seeded successfully.");

            // 7. Employees
            logger.LogInformation("7️⃣ Seeding Employees...");
            var employees = GenerateEmployees(branches, context);
            await context.Employees.AddRangeAsync(employees);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ Employees seeded successfully.");

            // 8. Coaches
            logger.LogInformation("8️⃣ Seeding Coaches...");
            var coaches = GenerateCoaches(employees, sports);
            await context.Set<Coach>().AddRangeAsync(coaches);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ Coaches seeded successfully.");

            // 9. Trainees
            logger.LogInformation("9️⃣ Seeding Trainees...");
            var trainees = GenerateTrainees(context);
            await context.Trainees.AddRangeAsync(trainees);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ Trainees seeded successfully.");

            // 10. TraineeGroups
            logger.LogInformation("🔟 Seeding TraineeGroups...");
            var traineeGroups = GenerateTraineeGroups(branches, coaches);
            await context.TraineeGroups.AddRangeAsync(traineeGroups);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ TraineeGroups seeded successfully.");

            // 11. GroupSchedules
            logger.LogInformation("1️⃣1️⃣ Seeding GroupSchedules...");
            var groupSchedules = GenerateGroupSchedules(traineeGroups);
            await context.Set<GroupSchedule>().AddRangeAsync(groupSchedules);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ GroupSchedules seeded successfully.");

            // 12. Payments
            logger.LogInformation("1️⃣2️⃣ Seeding Payments...");
            var payments = GeneratePayments(branches);
            await context.Set<Payment>().AddRangeAsync(payments);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ Payments seeded successfully.");

            // 13. SubscriptionDetails
            logger.LogInformation("1️⃣3️⃣ Seeding SubscriptionDetails...");
            var subscriptionDetails = GenerateSubscriptionDetails(trainees, subscriptionTypes, sports, branches, payments);
            await context.Set<SubscriptionDetails>().AddRangeAsync(subscriptionDetails);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ SubscriptionDetails seeded successfully.");

            // 14. Enrollments
            logger.LogInformation("1️⃣4️⃣ Seeding Enrollments...");
            var enrollments = GenerateEnrollments(trainees, traineeGroups, subscriptionDetails);
            await context.Enrollments.AddRangeAsync(enrollments);
            await context.SaveChangesAsync();
            logger.LogInformation("   ✔ Enrollments seeded successfully.");

            logger.LogInformation("🎉 Database seeding completed successfully!");
        }

        private static Address GenerateKuwaitiAddress(Faker faker)
        {
            var street = $"Street {faker.Random.Number(1, 250)}, Block {faker.Random.Number(1, 12)}";
            var city = faker.PickRandom(KuwaitiAreas);
            return Address.Create(street, city);
        }

        private static List<Branch> GenerateBranches()
        {
            var faker = new Faker<Branch>()
                .RuleFor(b => b.City, f => f.PickRandom(KuwaitiAreas))
                .RuleFor(b => b.Name, (f, b) => $"{b.City} Sports Academy")
                .RuleFor(b => b.Country, _ => "Kuwait")
                .RuleFor(b => b.PhoneNumber, f => $"{f.Random.Number(5, 9)}{f.Random.Number(1000000, 9999999)}")
                .RuleFor(b => b.Email, (f, b) => $"{b.Name.Replace(" ", "").Replace("-", "").ToLower()}{f.Random.Number(1, 999)}@sportacademy.com.kw")
                .RuleFor(b => b.CoX, f => f.Address.Latitude(29.0, 30.1).ToString())
                .RuleFor(b => b.CoY, f => f.Address.Longitude(47.5, 48.5).ToString())
                .RuleFor(b => b.IsActive, f => f.Random.Bool(0.9f));

            return faker.Generate(8);
        }

        private static List<Sport> GenerateSports()
        {
            var sports = new List<Sport>
            {
                new Sport
                {
                    Name = "Football",
                    Description = "The beautiful game - Soccer/Football training for all ages",
                    Category = SportCategory.Team,
                    IsRequireHealthTest = true
                },
                new Sport
                {
                    Name = "Basketball",
                    Description = "Indoor basketball training and competitive play",
                    Category = SportCategory.Team,
                    IsRequireHealthTest = true
                },
                new Sport
                {
                    Name = "Swimming",
                    Description = "Swimming lessons and water safety training",
                    Category = SportCategory.Individual,
                    IsRequireHealthTest = true
                },
                new Sport
                {
                    Name = "Volleyball",
                    Description = "Beach and indoor volleyball training",
                    Category = SportCategory.Team,
                    IsRequireHealthTest = false
                },
                new Sport
                {
                    Name = "Tennis",
                    Description = "Professional tennis coaching for all skill levels",
                    Category = SportCategory.Individual,
                    IsRequireHealthTest = false
                },
                new Sport
                {
                    Name = "Martial Arts",
                    Description = "Karate, Taekwondo, and self-defense training",
                    Category = SportCategory.Individual,
                    IsRequireHealthTest = true
                },
                new Sport
                {
                    Name = "Gymnastics",
                    Description = "Artistic gymnastics and flexibility training",
                    Category = SportCategory.Individual,
                    IsRequireHealthTest = true
                },
                new Sport
                {
                    Name = "Table Tennis",
                    Description = "Ping pong training and tournaments",
                    Category = SportCategory.Individual,
                    IsRequireHealthTest = false
                }
            };

            return sports;
        }

        private static List<SubscriptionType> GenerateSubscriptionTypes()
        {
            return new List<SubscriptionType>
            {
                new SubscriptionType
                {
                    Name = SubType.Basic,
                    DaysPerMonth = 8,
                    IsActive = true,
                    IsOffer = false
                },
                new SubscriptionType
                {
                    Name = SubType.Silver,
                    DaysPerMonth = 12,
                    IsActive = true,
                    IsOffer = false
                },
                new SubscriptionType
                {
                    Name = SubType.Gold,
                    DaysPerMonth = 16,
                    IsActive = true,
                    IsOffer = false
                },
                new SubscriptionType
                {
                    Name = SubType.Platinum,
                    DaysPerMonth = 24,
                    IsActive = true,
                    IsOffer = true
                },
                new SubscriptionType
                {
                    Name = SubType.Monthly,
                    DaysPerMonth = 20,
                    IsActive = true,
                    IsOffer = false
                }
            };
        }

        private static List<SportBranch> GenerateSportBranches(List<Sport> sports, List<Branch> branches)
        {
            var sportBranches = new List<SportBranch>();
            var random = new Random();

            foreach (var branch in branches)
            {
                // Each branch offers 4-6 random sports
                var numberOfSports = random.Next(4, 7);
                var selectedSports = sports.OrderBy(_ => random.Next()).Take(numberOfSports);

                foreach (var sport in selectedSports)
                {
                    sportBranches.Add(new SportBranch
                    {
                        SportId = sport.Id,
                        BranchId = branch.Id
                    });
                }
            }

            return sportBranches;
        }

        private static List<SportSubscriptionType> GenerateSportSubscriptionTypes(List<Sport> sports, List<SubscriptionType> subscriptionTypes)
        {
            var sportSubscriptionTypes = new List<SportSubscriptionType>();

            foreach (var sport in sports)
            {
                foreach (var subsType in subscriptionTypes)
                {
                    sportSubscriptionTypes.Add(new SportSubscriptionType
                    {
                        SportId = sport.Id,
                        SubscriptionTypeId = subsType.Id
                    });
                }
            }

            return sportSubscriptionTypes;
        }

        private static List<SportPrice> GenerateSportPrices(
            List<Sport> sports,
            List<Branch> branches,
            List<SubscriptionType> subscriptionTypes)
        {
            var sportPrices = new List<SportPrice>();
            var random = new Random();
            var basePrices = new Dictionary<string, decimal>
            {
                ["Football"] = 45m,
                ["Basketball"] = 40m,
                ["Swimming"] = 60m,
                ["Volleyball"] = 35m,
                ["Tennis"] = 55m,
                ["Martial Arts"] = 50m,
                ["Gymnastics"] = 65m,
                ["Table Tennis"] = 30m
            };

            foreach (var branch in branches)
            {
                foreach (var sport in sports)
                {
                    var basePrice = basePrices.GetValueOrDefault(sport.Name, 40m);

                    foreach (var subsType in subscriptionTypes)
                    {
                        var priceMultiplier = subsType.Name switch
                        {
                            SubType.Basic => 1.0m,
                            SubType.Silver => 1.4m,
                            SubType.Gold => 1.8m,
                            SubType.Platinum => 2.5m,
                            SubType.Monthly => 2.0m,
                            _ => 1.0m
                        };

                        sportPrices.Add(new SportPrice
                        {
                            SportId = sport.Id,
                            BranchId = branch.Id,
                            SubsTypeId = subsType.Id,
                            Price = Math.Round(basePrice * priceMultiplier + random.Next(-5, 5), 2)
                        });
                    }
                }
            }

            return sportPrices;
        }

        private static List<Employee> GenerateEmployees(List<Branch> branches, ApplicationDbContext context)
        {
            var users = context.Users.Take(50).Where(u => u.Id != null).ToList();
            if (users.Count < 50)
                throw new Exception("Not enough users. Please run user seeder first.");
            int i = 0;

            var faker = new Faker<Employee>()
                .RuleFor(e => e.FirstName, f => f.PickRandom(KuwaitiFirstNames.Concat(EgyptianFirstNames).ToList()))
                .RuleFor(e => e.LastName, f => f.PickRandom(ArabicLastNames))
                .RuleFor(e => e.SSN, f =>
                {
                    var year = f.Random.Number(1970, 2000) % 100;
                    var month = f.Random.Number(1, 12);
                    var day = f.Random.Number(1, 28);
                    var prefix = year > 99 ? 3 : 2;
                    return $"{prefix}{year:D2}{month:D2}{day:D2}{f.Random.Number(10000, 99999)}";
                })
                .RuleFor(e => e.Salary, f => f.Random.Decimal(400, 1500))
                .RuleFor(e => e.Gender, f => f.PickRandom<Gender>())
                .RuleFor(e => e.BirthDate, f => DateOnly.FromDateTime(f.Date.Past(35, DateTime.Now.AddYears(-18))))
                .RuleFor(e => e.HireDate, f => f.Date.Past(5))
                .RuleFor(e => e.Address, f => GenerateKuwaitiAddress(f)) // Using Address value object
                .RuleFor(e => e.Email, f => Email.Create(f.Internet.Email())) // Using Email value object
                .RuleFor(e => e.PhoneNumber, f => $"{f.Random.Number(5, 9)}{f.Random.Number(1000000, 9999999)}")
                .RuleFor(e => e.SecondPhoneNumber, f => f.Random.Bool(0.3f) ? $"{f.Random.Number(5, 9)}{f.Random.Number(1000000, 9999999)}" : null)
                .RuleFor(e => e.Nationality, f => f.PickRandom<Nationality>())
                .RuleFor(e => e.Position, f => f.PickRandom<Position>())
                .RuleFor(e => e.Branch, f => f.PickRandom(branches))
                .RuleFor(e => e.AppUser, f => users[++i]);

            return faker.Generate(30);
        }

        private static List<Coach> GenerateCoaches(List<Employee> employees, List<Sport> sports)
        {
            var coachEmployees = employees
                .Where(e => e.Position == Position.Coach)
                .Take(15)
                .ToList();

            int i = 0;
            var faker = new Faker<Coach>()
                .RuleFor(c => c.SkillLevel, f => f.PickRandom<SkillLevel>())
                .RuleFor(c => c.EmployeeId, f => coachEmployees[++i].Id)
                .RuleFor(c => c.SportId, f => f.PickRandom(sports).Id)
                .RuleFor(c => c.BirthDate, f => DateOnly.FromDateTime(f.Date.Past(25, DateTime.Now.AddYears(-22))));

            return faker.Generate(coachEmployees.Count - 1);
        }

        private static List<Trainee> GenerateTrainees(ApplicationDbContext context)
        {
            var users = context.Users.Where(u => u.Employee == null).Take(50).ToList();
            if (users.Count < 50)
                throw new Exception("Not enough users for trainees.");

            int i = 0;
            var faker = new Faker<Trainee>()
                .RuleFor(t => t.FirstName, f => f.PickRandom(KuwaitiFirstNames.Concat(EgyptianFirstNames).ToList()))
                .RuleFor(t => t.LastName, f => f.PickRandom(ArabicLastNames))
                .RuleFor(t => t.SSN, f =>
                {
                    var year = f.Random.Number(2000, 2015) % 100;
                    var month = f.Random.Number(1, 12);
                    var day = f.Random.Number(1, 28);
                    return $"3{year:D2}{month:D2}{day:D2}{f.Random.Number(10000, 99999)}";
                })
                .RuleFor(t => t.BirthDate, f => DateOnly.FromDateTime(f.Date.Past(18, DateTime.Now.AddYears(-6))))
                .RuleFor(t => t.Gender, f => f.PickRandom<Gender>())
                .RuleFor(t => t.Address, f => GenerateKuwaitiAddress(f)) // Using Address value object
                .RuleFor(t => t.Email, f => Email.Create(f.Internet.Email())) // Using Email value object
                .RuleFor(t => t.PhoneNumber, f => $"{f.Random.Number(5, 9)}{f.Random.Number(1000000, 9999999)}")
                .RuleFor(t => t.Nationality, f => f.PickRandom<Nationality>())
                .RuleFor(t => t.IsSubscribed, f => f.Random.Bool(0.7f))
                .RuleFor(t => t.ParentNumber, (f, t) =>
                {
                    var age = DateTime.Now.Year - t.BirthDate.Year;
                    return age < 15 ? $"{f.Random.Number(5, 9)}{f.Random.Number(1000000, 9999999)}" : null;
                })
                .RuleFor(t => t.GuardianName, (f, t) =>
                {
                    var age = DateTime.Now.Year - t.BirthDate.Year;
                    return age < 15 ? $"{f.PickRandom(KuwaitiFirstNames)} {f.PickRandom(ArabicLastNames)}" : null;
                })
                .RuleFor(t => t.AppUserId, f => users[i++].Id);

            var trainees = faker.Generate(50);

            // Generate trainee IDs manually based on business logic
            var random = new Random();
            foreach (var trainee in trainees)
            {
                var branchId = random.Next(1, 9);
                var year = trainee.BirthDate.Year % 100;
                var month = trainee.BirthDate.Month;
                var dobCode = $"{year:D2}{month:D2}";
                var firstLetter = char.ToUpper(trainee.FirstName[0]);
                var ascii = ((int)firstLetter).ToString("D2");
                var prefix = $"{branchId}{dobCode}{ascii}";
                var counter = random.Next(1, 99).ToString("D2");
                trainee.Id = int.Parse($"{prefix}{counter}");
            }

            return trainees;
        }

        private static List<TraineeGroup> GenerateTraineeGroups(List<Branch> branches, List<Coach> coaches)
        {
            var faker = new Faker<TraineeGroup>()
                .RuleFor(tg => tg.SkillLevel, f => f.PickRandom<SkillLevel>())
                .RuleFor(tg => tg.MaximumCapacity, f => f.Random.Number(10, 15))
                .RuleFor(tg => tg.DurationInMinutes, f => f.Random.Number(45, 90))
                .RuleFor(tg => tg.Gender, f => f.PickRandom<Gender>())
                .RuleFor(tg => tg.BranchId, f => f.PickRandom(branches).Id)
                .RuleFor(tg => tg.CoachId, f => f.PickRandom(coaches).EmployeeId);

            return faker.Generate(25);
        }

        private static List<GroupSchedule> GenerateGroupSchedules(List<TraineeGroup> traineeGroups)
        {
            var groupSchedules = new List<GroupSchedule>();
            var random = new Random();

            foreach (var group in traineeGroups)
            {
                // Each group has 2-3 sessions per week
                var numberOfSessions = random.Next(2, 4);
                var availableDays = Enum.GetValues<DayOfWeek>().Where(d => d != DayOfWeek.Friday).ToList();
                var selectedDays = availableDays.OrderBy(_ => random.Next()).Take(numberOfSessions);

                foreach (var day in selectedDays)
                {
                    var hour = random.Next(8, 20);
                    var minute = random.Next(0, 2) * 30; // 0 or 30

                    groupSchedules.Add(new GroupSchedule
                    {
                        TraineeGroupId = group.Id,
                        Day = day,
                        StartTime = new TimeOnly(hour, minute)
                    });
                }
            }

            return groupSchedules;
        }

        private static List<Payment> GeneratePayments(List<Branch> branches)
        {
            var payments = new List<Payment>();
            var random = new Random();

            for (int i = 0; i < 100; i++)
            {
                var paymentNumber = $"PAY-{DateTime.UtcNow.Year}-{random.Next(10000, 99999)}";

                payments.Add(new Payment
                {
                    PaymentNumber = paymentNumber,
                    Method = random.Next(0, 2) == 0 ? PaymentMethod.Cash : PaymentMethod.Online,
                    PaidDate = DateTime.Now.AddDays(-random.Next(1, 180)),
                    BranchId = branches[random.Next(branches.Count)].Id
                });
            }

            return payments;
        }

        private static List<SubscriptionDetails> GenerateSubscriptionDetails(
            List<Trainee> trainees,
            List<SubscriptionType> subscriptionTypes,
            List<Sport> sports,
            List<Branch> branches,
            List<Payment> payments)
        {
            var subscriptionDetails = new List<SubscriptionDetails>();
            var random = new Random();
            var subscribedTrainees = trainees.Where(t => t.IsSubscribed).ToList();

            for (int i = 0; i < subscribedTrainees.Count && i < payments.Count; i++)
            {
                var trainee = subscribedTrainees[i];
                var payment = payments[i];
                var startDate = DateOnly.FromDateTime(payment.PaidDate);
                var subsType = subscriptionTypes[random.Next(subscriptionTypes.Count)];
                var sport = sports[random.Next(sports.Count)];
                var branch = branches[random.Next(branches.Count)];

                subscriptionDetails.Add(new SubscriptionDetails
                {
                    StartDate = startDate,
                    EndDate = startDate.AddMonths(1),
                    PaymentNumber = payment.PaymentNumber,
                    TraineeId = trainee.Id,
                    SubscriptionTypeId = subsType.Id,
                    SportId = sport.Id,
                    BranchId = branch.Id
                });
            }

            return subscriptionDetails;
        }

        private static List<Enrollment> GenerateEnrollments(
            List<Trainee> trainees,
            List<TraineeGroup> traineeGroups,
            List<SubscriptionDetails> subscriptionDetails)
        {
            var enrollments = new List<Enrollment>();
            var random = new Random();

            foreach (var subDetail in subscriptionDetails)
            {
                var group = traineeGroups[random.Next(traineeGroups.Count)];

                var enrollment = new Enrollment
                {
                    EnrollmentDate = subDetail.StartDate.ToDateTime(TimeOnly.MinValue),
                    ExpiryDate = subDetail.EndDate.ToDateTime(TimeOnly.MinValue),
                    SessionAllowed = subDetail.SportPrice.SportSubscriptionType.SubscriptionType.DaysPerMonth,
                    SessionRemaining = random.Next(0, subDetail.SportPrice.SportSubscriptionType.SubscriptionType.DaysPerMonth + 1),
                    IsActive = true,
                    TraineeId = subDetail.TraineeId,
                    TraineeGroupId = group.Id,
                    SubscriptionDetailsId = subDetail.Id
                };

                enrollments.Add(enrollment);
            }

            return enrollments;
        }
    }
}