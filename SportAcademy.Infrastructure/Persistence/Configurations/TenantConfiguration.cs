using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(t => t.Code)
            .IsUnique();

        builder.HasIndex(t => t.Slug)
            .IsUnique();

        builder.HasIndex(t => t.Name);

        builder.HasIndex(t => t.Status);

        // Owner
        builder.HasOne(t => t.Owner)
            .WithMany()
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Users
        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Profile (1 : 1)
        builder.HasOne(t => t.Profile)
            .WithOne(p => p.Tenant)
            .HasForeignKey<TenantProfile>(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Subscription (1 : 1)
        builder.HasOne(t => t.Subscription)
            .WithOne(s => s.Tenant)
            .HasForeignKey<TenantSubscription>(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Branches (1 : Many)
        builder.HasMany(t => t.Branches)
            .WithOne(b => b.Tenant)
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Features (Many : Many via TenantFeature)
        builder.HasMany(t => t.Features)
            .WithOne(tf => tf.Tenant)
            .HasForeignKey(tf => tf.TenantId);
    }
}