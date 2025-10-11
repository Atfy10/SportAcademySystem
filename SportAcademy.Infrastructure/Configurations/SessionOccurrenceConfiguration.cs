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
    public class SessionOccurrenceConfiguration : IEntityTypeConfiguration<SessionOccurrence>
    {
        public void Configure(EntityTypeBuilder<SessionOccurrence> builder)
        {
            builder.ToTable("SessionOccurrences");

            builder.HasKey(so => so.Id);

            builder.Property(so => so.StartDateTime)
                .IsRequired();

            builder.Property(so => so.Status)
                .IsRequired()
                .HasConversion<string>();

            // Relationships
            // 1:M Attendance
            builder.HasMany(so => so.Attendances)
                .WithOne(a => a.SessionOccurrence)
                .HasForeignKey(a => a.SessionOccurrenceId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1:M GroupSchedule
            builder.HasOne(so => so.GroupSchedule)
                .WithMany(gs => gs.SessionOccurrences)
                .HasForeignKey(so => so.GroupScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
