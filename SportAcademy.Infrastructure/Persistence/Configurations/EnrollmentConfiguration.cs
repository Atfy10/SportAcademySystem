using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            // Table Name
            builder.ToTable("Enrollments");

            // PK
            builder.HasKey(e => e.Id);

            // Props
            builder.Property(e => e.EnrollmentDate)
                   .IsRequired();

            builder.Property(e => e.ExpiryDate)
                   .IsRequired();

            // Relationships

            // 1:M Trainee
            builder.HasOne(e => e.Trainee)
                   .WithMany(t => t.Enrollments)
                   .HasForeignKey(e => e.TraineeId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1:M TraineeGroup
            builder.HasOne(e => e.TraineeGroup)
                   .WithMany(s => s.Enrollments)
                   .HasForeignKey(e => e.TraineeGroupId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1:M Attendances
            builder.HasMany(e => e.Attendances)
                   .WithOne(a => a.Enrollment)
                   .HasForeignKey(a => a.EnrollmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1:1 SubscriptionDetails
            builder.HasOne(e => e.SubscriptionDetails)
                   .WithOne(sd => sd.Enrollment)
                   .HasForeignKey<Enrollment>(e => e.SubscriptionDetailsId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
