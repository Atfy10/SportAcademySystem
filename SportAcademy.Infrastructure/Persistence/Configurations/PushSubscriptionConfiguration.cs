using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
    {
        public void Configure(EntityTypeBuilder<PushSubscription> builder)
        {
            builder.ToTable("PushSubscriptions");

            builder.HasKey(x => x.Id);

            // Real-world push endpoints (FCM, Mozilla, Apple's web push) run 150-300 characters -
            // 512 leaves headroom while staying safely under SQL Server's ~1700-byte unique-index
            // key limit (512 nvarchar chars = 1024 bytes); the 2048 this started at would have
            // been rejected the first time the index actually had to enforce uniqueness.
            builder.Property(x => x.Endpoint)
                .IsRequired()
                .HasMaxLength(512);

            // A given endpoint (one physical push channel) can only be subscribed once - the
            // browser reuses the same endpoint across repeat subscribe() calls for the same
            // device/profile, so this also makes re-registering idempotent.
            builder.HasIndex(x => x.Endpoint)
                .IsUnique()
                .HasDatabaseName("IX_PushSubscription_Endpoint");

            builder.Property(x => x.P256dh)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Auth)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.UserAgent)
                .HasMaxLength(512);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_PushSubscription_UserId");

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
