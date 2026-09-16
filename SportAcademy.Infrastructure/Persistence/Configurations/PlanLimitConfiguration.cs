using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Infrastructure.Persistence.Configurations;

public class PlanLimitConfiguration : IEntityTypeConfiguration<PlanLimit>
{
    public void Configure(EntityTypeBuilder<PlanLimit> builder)
    {
        builder.ToTable("PlanLimits");

        builder.HasKey(pl => pl.Id);

        builder.Property(pl => pl.ResourceKey)
            .IsRequired()
            .HasMaxLength(50);

        // One limit row per (plan, resource) - the same shape as
        // SubscriptionPlanFeatureConfiguration's composite key on (plan, feature), except
        // ResourceKey is a string rather than an id, so this needs an explicit unique index
        // rather than a composite PK.
        builder.HasIndex(pl => new { pl.SubscriptionPlanId, pl.ResourceKey })
            .IsUnique();

        builder.HasOne(pl => pl.SubscriptionPlan)
            .WithMany(sp => sp.Limits)
            .HasForeignKey(pl => pl.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
