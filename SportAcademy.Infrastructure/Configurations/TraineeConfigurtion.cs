using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SportAcademy.Domain.Entities;
using System.Reflection.Emit;

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

            builder.Property(t => t.Id)
                .ValueGeneratedNever();

            // Props
            #region Person Properties
            builder.OwnsOne(t => t.Address, eb =>
                {
                    eb.Property(a => a.Street)
                    .HasColumnName("Street")
                    .IsRequired()
                    .HasMaxLength(70);

                    eb.Property(a => a.City)
                        .HasColumnName("City")
                        .IsRequired()
                        .HasMaxLength(50);
                });

            builder.OwnsOne(t => t.Email, emailBuilder =>
                {
                    emailBuilder.Property(e => e.Value)
                        .HasColumnName("Email")
                        .HasMaxLength(200)
                        .IsRequired();

                    emailBuilder.WithOwner();
                });
            #endregion

            builder.Property(t => t.IsSubscribed)
                   .IsRequired();

            builder.Property(t => t.ParentNumber)
                   .HasMaxLength(13);

            builder.Property(t => t.GuardianName)
                .HasMaxLength(50);

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
