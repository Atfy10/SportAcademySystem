using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;
using System.Linq.Expressions;
using System.Reflection;

namespace SportAcademy.Infrastructure.Persistence.DBContext
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
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
        public DbSet<SportPrice> SportPrices { get; set; }
        public DbSet<SportSubscriptionType> SportSubscriptionTypes { get; set; }
        public DbSet<SportTrainee> SportTrainees { get; set; }
        public DbSet<SubscriptionDetails> SubscriptionDetails { get; set; }
        public DbSet<SubscriptionType> SubscriptionTypes { get; set; }
        public DbSet<Trainee> Trainees { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

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
        }
    }
}
