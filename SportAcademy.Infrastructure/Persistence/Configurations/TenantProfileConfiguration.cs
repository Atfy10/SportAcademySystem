using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Infrastructure.Persistence.Configurations;

public sealed class TenantProfileConfiguration : IEntityTypeConfiguration<TenantProfile>
{
    public void Configure(EntityTypeBuilder<TenantProfile> builder)
    {
        builder.ToTable("TenantProfiles");

        builder.HasKey(tp => tp.TenantId);

        builder.Property(tp => tp.OrganizationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(tp => tp.LogoUrl)
            .HasMaxLength(500);

        builder.Property(tp => tp.Email)
            .HasMaxLength(256);

        builder.Property(tp => tp.Phone)
            .HasMaxLength(30);

        builder.Property(tp => tp.Website)
            .HasMaxLength(250);

        builder.Property(tp => tp.Address)
            .HasMaxLength(500);

        builder.Property(tp => tp.TaxNumber)
            .HasMaxLength(100);

        builder.Property(tp => tp.CommercialRegistration)
            .HasMaxLength(100);

        builder.Property(tp => tp.Description)
            .HasMaxLength(2000);

        builder.HasOne(tp => tp.Tenant)
            .WithOne(t => t.Profile)
            .HasForeignKey<TenantProfile>(tp => tp.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}