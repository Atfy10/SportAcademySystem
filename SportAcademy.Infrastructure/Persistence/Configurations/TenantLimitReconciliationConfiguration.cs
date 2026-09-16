using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Infrastructure.Persistence.Configurations;

public class TenantLimitReconciliationConfiguration : IEntityTypeConfiguration<TenantLimitReconciliation>
{
    public void Configure(EntityTypeBuilder<TenantLimitReconciliation> builder)
    {
        builder.ToTable("TenantLimitReconciliations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RequiredResourcesJson)
            .IsRequired();

        // Fast lookup for "does this tenant already have an open one" (LimitReconciliationService)
        // and for the deadline sweep's "every open one past its deadline" query.
        builder.HasIndex(r => new { r.TenantId, r.CompletedAt });
        builder.HasIndex(r => new { r.CompletedAt, r.DeadlineAt });

        // Not ITenantScoped (see the entity's own comment) - configured by hand exactly like
        // TenantFeatureConfiguration/TenantLimitOverrideConfiguration.
        builder.HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
