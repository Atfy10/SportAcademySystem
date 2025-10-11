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
    public class GroupScheduleConfiguration : IEntityTypeConfiguration<GroupSchedule>
    {
        public void Configure(EntityTypeBuilder<GroupSchedule> builder)
        {
            builder.ToTable("GroupSchedules");

            builder.HasKey(gs => gs.Id);

            builder.Property(gs => gs.Day)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(gs => gs.StartTime)
                .IsRequired();

            // Relationships
            // 1:M TraineeGroup
            builder.HasOne(gs => gs.TraineeGroup)
                .WithMany(tg => tg.GroupSchedules)
                .HasForeignKey(gs => gs.TraineeGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1:M SessionOccurrences
            builder.HasMany(gs => gs.SessionOccurrences)
                .WithOne(so => so.GroupSchedule)
                .HasForeignKey(so => so.GroupScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
