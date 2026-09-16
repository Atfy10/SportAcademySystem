using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Infrastructure.Persistence.Configurations;

public class TenantLimitOverrideConfiguration : IEntityTypeConfiguration<TenantLimitOverride>
{
    public void Configure(EntityTypeBuilder<TenantLimitOverride> builder)
    {
        builder.ToTable("TenantLimitOverrides");

        builder.HasKey(o => new { o.TenantId, o.ResourceKey });

        builder.Property(o => o.ResourceKey)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.SetBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.SetAt)
            .IsRequired();

        builder.Property(o => o.Reason)
            .HasMaxLength(500);

        // Not ITenantScoped (see the entity's own comment), so this relationship is configured
        // by hand exactly like TenantFeatureConfiguration configures Tenant<->TenantFeature -
        // the dynamic ITenantScoped loop in ApplicationDbContext.OnModelCreating never sees this
        // entity to wire it up automatically.
        builder.HasOne(o => o.Tenant)
            .WithMany()
            .HasForeignKey(o => o.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
