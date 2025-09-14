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
    public class TraineeConfigurtion : IEntityTypeConfiguration<Trainee>
    {
        public void Configure(EntityTypeBuilder<Trainee> builder)
        {
            // TableName 
            builder.ToTable("Trainees");

            //PK
            builder.HasKey(t => t.Id);

            // Props
            builder.Property(t => t.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.SSN)
                .IsRequired()
                .HasMaxLength(14);

            builder.Property(t => t.IsSubscribed)
                   .IsRequired();

            builder.Property(t => t.ParentNumber)
                   .HasMaxLength(13);

            builder.Property(t => t.GuardianName)
                .HasMaxLength(50);

            builder.Property(t => t.BirthDate)
                .IsRequired();

            builder.Property(t => t.Gender)
                .HasConversion<string>() 
                .IsRequired();

            // Relationships

            // 1:1  AppUser
            builder.HasOne(t => t.AppUser)
                .WithOne(u => u.Trainee)
                .HasForeignKey<Trainee>(t => t.AppUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M  SportTrainee
            builder.HasMany(t => t.Sports)
                .WithOne(st => st.Trainee)
                .HasForeignKey(st => st.TraineeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M  Enrollment
            builder.HasMany(t => t.Enrollments)
                .WithOne(e => e.Trainee)
                .HasForeignKey(e => e.TraineeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M  SubscriptionDetails
            builder.HasMany(t => t.SubscriptionDetails)
                .WithOne(sd => sd.Trainee)
                .HasForeignKey(sd => sd.TraineeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
