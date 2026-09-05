using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class TraineeGroupConfiguration : IEntityTypeConfiguration<TraineeGroup>
    {
        public void Configure(EntityTypeBuilder<TraineeGroup> builder)
        {
            builder.ToTable("TraineeGroups");

            builder.HasKey(tg => tg.Id);

            builder.Property(tg => tg.Name)
                .IsRequired()
                .HasDefaultValue("Trainee Group")
                .ValueGeneratedOnAdd()
                .HasMaxLength(200);

            builder.Property(tg => tg.SkillLevel)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(tg => tg.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(TraineeGroupType.Public);

            builder.Property(tg => tg.MaximumCapacity)
                .IsRequired()
                .HasDefaultValue(15);

            builder.Property(tg => tg.DurationInMinutes)
                .IsRequired()
                .HasDefaultValue(55);

            builder.Property(tg => tg.Gender)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(tg => tg.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(tg => tg.InactiveReason)
                .HasMaxLength(500);

            // Relationships
            // M:1  Coach
            builder.HasOne(tg => tg.Coach)
                .WithMany(c => c.TraineeGroups)
                .HasForeignKey(tg => tg.CoachId)
                .OnDelete(DeleteBehavior.Restrict);

            // M:1  Sport
            builder.HasOne(tg => tg.Branch)
                .WithMany(s => s.TraineeGroups)
                .HasForeignKey(tg => tg.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1:M GroupSchedules - Cascade: deleting a group must delete its weekly schedule
            // slots (and, transitively, their SessionOccurrences - see
            // SessionOccurrenceConfiguration) too. Configured here only - GroupScheduleConfiguration
            // used to also configure this same relationship with Restrict, and since both
            // configs target the same FK, whichever IEntityTypeConfiguration happened to apply
            // last silently won - Restrict was winning, so deleting a group with any schedule
            // rows failed outright.
            builder.HasMany(tg => tg.GroupSchedules)
                .WithOne(gs => gs.TraineeGroup)
                .HasForeignKey(gs => gs.TraineeGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1:M Enrollments
            builder.HasMany(tg => tg.Enrollments)
                .WithOne(e => e.TraineeGroup)
                .HasForeignKey(e => e.TraineeGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
