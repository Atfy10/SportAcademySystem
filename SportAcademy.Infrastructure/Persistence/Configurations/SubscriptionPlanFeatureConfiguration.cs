using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanFeatureConfiguration : IEntityTypeConfiguration<SubscriptionPlanFeature>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlanFeature> builder)
    {
        builder.ToTable("SubscriptionPlanFeatures");

        builder.HasKey(spf => new { spf.SubscriptionPlanId, spf.FeatureId });

        builder.HasOne(spf => spf.SubscriptionPlan)
            .WithMany(sp => sp.Features)
            .HasForeignKey(spf => spf.SubscriptionPlanId);

        builder.HasOne(spf => spf.Feature)
            .WithMany(f => f.SubscriptionPlans)
            .HasForeignKey(spf => spf.FeatureId);
    }
}
