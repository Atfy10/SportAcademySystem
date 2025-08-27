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
    public class AttendanceConfigurtion : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            // Table Name
            builder.ToTable("Attendances");

            // PK
            builder.HasKey(a => a.Id);

            //props
            builder.Property(a => a.AttendanceDate)
                  .IsRequired();

            builder.Property(a => a.IsPresent)
                   .IsRequired();

            builder.Property(a => a.CheckInTime)
                   .IsRequired();

            builder.Property(a => a.CoachNote)
                   .IsRequired()
                   .HasMaxLength(500);


            // Relationship
            // 1:M Enrollment

            builder.HasOne(a => a.Enrollment)
                  .WithMany(e => e.Attendances)
                  .HasForeignKey(a => a.EnrollmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
