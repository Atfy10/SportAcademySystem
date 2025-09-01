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
    public class CoachConfiguration : IEntityTypeConfiguration<Coach>
    {
        public void Configure(EntityTypeBuilder<Coach> builder)
        {
            // Table Name
            builder.ToTable("Coaches");

            // PK
            builder.HasKey(c => c.EmployeeId);

            // Props
            builder.Property(c => c.SkillLevel)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(c => c.BirthDate)
                .IsRequired();

            // Relationships
            // 1:1  Employee
            builder.HasOne(c => c.Employee)
                .WithOne(e => e.Coach)
                .HasForeignKey<Coach>(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // M:1  Sport
            builder.HasOne(c => c.Sport)
                .WithMany(s => s.Coaches)
                .HasForeignKey(c => c.SportId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
