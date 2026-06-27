using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class TenantFeatureConfiguration : IEntityTypeConfiguration<TenantFeature>
    {
        public void Configure(EntityTypeBuilder<TenantFeature> builder)
        {
            builder.ToTable("TenantFeatures");

            builder.HasKey(tf => new
            {
                tf.TenantId,
                tf.FeatureId
            });

            builder.Property(tf => tf.IsEnabled)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(tf => tf.EnabledAt)
                .IsRequired();

            builder.Property(tf => tf.EnabledBy)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("System");

            builder.HasOne(tf => tf.Tenant)
                .WithMany(t => t.Features)
                .HasForeignKey(tf => tf.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tf => tf.Feature)
                .WithMany(f => f.TenantFeatures)
                .HasForeignKey(tf => tf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(tf => tf.IsEnabled);
        }
    }
}
