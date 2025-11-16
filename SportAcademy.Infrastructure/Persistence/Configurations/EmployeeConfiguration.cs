using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            // Table Name
            builder.ToTable("Employees");

            // PK
            builder.HasKey(e => e.Id);

            // Props
            #region Person Properties
            builder.OwnsOne(e => e.Address, ab =>
            {
                ab.Property(a => a.Street)
                .HasColumnName("Street")
                .IsRequired()
                .HasMaxLength(70);

                ab.Property(a => a.City)
                  .HasColumnName("City")
                  .IsRequired()
                  .HasMaxLength(50);

                ab.WithOwner();
            });

            builder.OwnsOne(e => e.Email, emailBuilder =>
            {
                emailBuilder.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(200)
                    .IsRequired();

                emailBuilder.WithOwner();
            });
            #endregion

            builder.Property(e => e.Position)
                .HasConversion<string>();

            builder.Property(e => e.Salary)
                .HasPrecision(18, 2);

            builder.Property(e => e.HireDate)
                .IsRequired();

            builder.Property(e => e.AppUserId)
                .IsRequired();

            builder.HasIndex(e => e.AppUserId)
                .IsUnique();

            // Relationships
            // 1:1   AppUser
            builder.HasOne(e => e.AppUser)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:1   Branch
            builder.HasOne(e => e.Branch)
                .WithMany(b => b.Employees)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:1   Coach
            builder.HasOne(e => e.Coach)
                .WithOne(c => c.Employee)
                .HasForeignKey<Coach>(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
