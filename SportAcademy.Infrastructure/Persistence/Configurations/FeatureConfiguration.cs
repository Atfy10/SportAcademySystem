using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
    {
        public void Configure(EntityTypeBuilder<Feature> builder)
        {
            builder.ToTable("Features");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id)
                .ValueGeneratedOnAdd();

            builder.Property(f => f.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(f => f.Name)
                .IsUnique()
                .HasDatabaseName("IX_Feature_Name");

            builder.Property(f => f.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(f => f.Description)
                .HasMaxLength(500);

            builder.Property(f => f.CreatedAt)
                .IsRequired();

            builder.Property(f => f.IsImplemented)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(f => f.BundlePrice)
                .IsRequired()
                .HasPrecision(10, 2)
                .HasDefaultValue(0m);

            builder.Property(f => f.IsBundleCore)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasMany(f => f.TenantFeatures)
                .WithOne(cf => cf.Feature)
                .HasForeignKey(cf => cf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
