using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Infrastructure.Persistence.Configurations;

public sealed class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> builder)
    {
        builder.ToTable("TenantSettings");

        builder.HasKey(ts => ts.TenantId);

        builder.Property(ts => ts.TimeZone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("Asia/Kuwait");

        builder.Property(ts => ts.Language)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("ar-KW");

        builder.Property(ts => ts.DateFormat)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("dd/MM/yyyy");

        builder.Property(ts => ts.TimeFormat)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("HH:mm");

        builder.Property(ts => ts.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("KWD");

        builder.HasOne(ts => ts.Tenant)
            .WithOne(t => t.Settings)
            .HasForeignKey<TenantSettings>(ts => ts.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}