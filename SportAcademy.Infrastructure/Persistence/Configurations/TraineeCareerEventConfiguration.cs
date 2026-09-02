using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class TraineeCareerEventConfiguration : IEntityTypeConfiguration<TraineeCareerEvent>
    {
        public void Configure(EntityTypeBuilder<TraineeCareerEvent> builder)
        {
            // Table Name
            builder.ToTable("TraineeCareerEvents");

            // PK
            builder.HasKey(e => e.Id);

            // Props
            builder.Property(e => e.EventType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(e => e.SkillLevel)
                .HasConversion<string>();

            builder.Property(e => e.Reason)
                .HasMaxLength(500);

            builder.Property(e => e.EffectiveDate)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            // Relationships
            // M:1  Trainee
            builder.HasOne(e => e.Trainee)
                .WithMany(t => t.CareerEvents)
                .HasForeignKey(e => e.TraineeId)
                .OnDelete(DeleteBehavior.Cascade);

            // M:1  Sport
            builder.HasOne(e => e.Sport)
                .WithMany()
                .HasForeignKey(e => e.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            // M:1  TraineeGroup
            builder.HasOne(e => e.TraineeGroup)
                .WithMany()
                .HasForeignKey(e => e.TraineeGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // M:1  Coach
            builder.HasOne(e => e.Coach)
                .WithMany()
                .HasForeignKey(e => e.CoachId)
                .OnDelete(DeleteBehavior.Restrict);

            // M:1  Enrollment
            builder.HasOne(e => e.Enrollment)
                .WithMany()
                .HasForeignKey(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(e => new { e.TraineeId, e.EventType, e.EffectiveDate });
            builder.HasIndex(e => new { e.TraineeId, e.SportId, e.EventType, e.EffectiveDate });
        }
    }
}
