using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Configurations;

public sealed class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("TenantSubscriptions");

        builder.HasKey(ts => ts.TenantId);

        builder.Property(ts => ts.SubscriptionPlanId)
            .IsRequired();

        builder.Property(ts => ts.StartsAt)
            .IsRequired();

        builder.Property(ts => ts.EndsAt)
            .IsRequired();

        builder.Property(ts => ts.IsTrial)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ts => ts.AutoRenew)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(ts => ts.Tenant)
            .WithOne(t => t.Subscription)
            .HasForeignKey<TenantSubscription>(ts => ts.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ts => ts.Plan)
            .WithMany(sp => sp.Subscriptions);
    }
}