using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class TenantAuditEventConfiguration : IEntityTypeConfiguration<TenantAuditEvent>
    {
        public void Configure(EntityTypeBuilder<TenantAuditEvent> builder)
        {
            builder.ToTable("TenantAuditEvents");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.Outcome)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(e => e.Reason)
                .HasMaxLength(500);

            // AfterJson/BeforeJson are payload snapshots, not queried on - nvarchar(max) rather
            // than an arbitrary length cap that could silently truncate a large command.
            builder.Property(e => e.AfterJson);
            builder.Property(e => e.BeforeJson);

            builder.Property(e => e.PerformedByUserId)
                .IsRequired();

            builder.Property(e => e.PerformedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.IpAddress)
                .HasMaxLength(45); // IPv6 textual max length

            builder.Property(e => e.UserAgent)
                .HasMaxLength(500);

            builder.Property(e => e.CorrelationId)
                .HasMaxLength(64);

            builder.Property(e => e.PerformedAt)
                .IsRequired();

            builder.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.TenantId);
            builder.HasIndex(e => e.PerformedAt);
            builder.HasIndex(e => e.EventType);
            builder.HasIndex(e => e.PerformedByUserId);
        }
    }
}
