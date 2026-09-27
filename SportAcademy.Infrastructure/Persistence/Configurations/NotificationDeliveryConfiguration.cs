using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
    {
        public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
        {
            builder.ToTable("NotificationDeliveries");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Channel)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(Domain.Enums.NotificationDeliveryStatus.Pending);

            builder.Property(x => x.AttemptCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.NextAttemptAt)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.LastError)
                .HasMaxLength(1000);

            builder.Property(x => x.ResolvedDestination)
                .HasMaxLength(320);

            // The worker's claim query filters on (Status, NextAttemptAt) every poll - this is
            // its only index, and it's a hot path.
            builder.HasIndex(x => new { x.Status, x.NextAttemptAt })
                .HasDatabaseName("IX_NotificationDelivery_Status_NextAttemptAt");

            builder.HasOne(x => x.Notification)
                .WithMany()
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
