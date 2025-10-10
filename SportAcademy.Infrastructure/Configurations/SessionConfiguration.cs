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
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            // Table Name
            builder.ToTable("Sessions");

            // PK
            builder.HasKey(s => s.Id);

            // Props
            builder.Property(s => s.SkillLevel)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(s => s.Day)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(9);

            builder.Property(s => s.Gender)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(s => s.MaximumCapacity)
                .HasDefaultValue(15);

            builder.Property(s => s.DurationInMinutes)
                .HasDefaultValue(55);

            builder.Property(s => s.Date)
                .IsRequired();

            builder.Property(s => s.StartTime)
                .IsRequired();

            // Relationships

            //  1:M Branch
            builder.HasOne(s => s.Branch)
                .WithMany(b => b.Sessions)
                .HasForeignKey(s => s.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            //  1:M Coach
            builder.HasOne(s => s.Coach)
                .WithMany(c => c.Sessions)
                .HasForeignKey(s => s.CoachId)
                .OnDelete(DeleteBehavior.Restrict);

            //  1:M Enrollments
            builder.HasMany(s => s.Enrollments)
                .WithOne(e => e.Session)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
