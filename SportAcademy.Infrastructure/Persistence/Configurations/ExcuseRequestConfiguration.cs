using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class ExcuseRequestConfiguration : IEntityTypeConfiguration<ExcuseRequest>
    {
        public void Configure(EntityTypeBuilder<ExcuseRequest> builder)
        {
            builder.ToTable("ExcuseRequests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.ReviewNote)
                .HasMaxLength(500);

            builder.Property(x => x.RequestedByUserId)
                .HasMaxLength(450);

            builder.Property(x => x.ReviewedByUserId)
                .HasMaxLength(450);

            // Relationships - Restrict everywhere: an excuse request is an approval audit trail,
            // it must never silently vanish because something it references got deleted.
            builder.HasOne(x => x.SessionOccurrence)
                .WithMany()
                .HasForeignKey(x => x.SessionOccurrenceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Trainee)
                .WithMany()
                .HasForeignKey(x => x.TraineeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Enrollment)
                .WithMany()
                .HasForeignKey(x => x.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
