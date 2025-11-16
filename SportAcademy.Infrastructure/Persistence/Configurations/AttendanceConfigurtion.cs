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
    public class AttendanceConfigurtion : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            //  Table Name
            builder.ToTable("Attendances");

            //  PK
            builder.HasKey(a => a.Id);

            //  props
            builder.Property(a => a.AttendanceDate)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(a => a.AttendanceStatus)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(a => a.CheckInTime)
                .HasDefaultValueSql("CONVERT(TIME, GETUTCDATE())")
                .IsRequired();

            builder.Property(a => a.CoachNote)
                .IsRequired()
                .HasMaxLength(500);

            //  Relationship
            //  1:M Enrollment
            builder.HasOne(a => a.Enrollment)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            //  1:M SessionOccurrence
            builder.HasOne(a => a.SessionOccurrence)
                .WithMany(so => so.Attendances)
                .HasForeignKey(a => a.SessionOccurrenceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
