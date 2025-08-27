using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Configurations
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
            builder.Property(e => e.FirstName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.LastName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.SSN)
                   .IsRequired()
                   .HasMaxLength(14);

            builder.Property(e => e.Address)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(e => e.SecondPhoneNumber)
                   .HasMaxLength(20);

            //builder.Property(e => e.Position)
            //       .HasDefaultValue("Employee");

            builder.Property(e => e.Salary)
                   .HasColumnType("decimal(18,2)");

            builder.Property(e => e.AppUserId)
                   .IsRequired();

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
