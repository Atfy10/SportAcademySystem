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

            builder.Property(e => e.PerformedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.PerformedAt)
                .IsRequired();

            builder.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.TenantId);
            builder.HasIndex(e => e.PerformedAt);
        }
    }
}
