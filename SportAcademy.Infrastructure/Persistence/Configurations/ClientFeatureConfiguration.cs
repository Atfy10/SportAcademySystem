using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class ClientFeatureConfiguration : IEntityTypeConfiguration<ClientFeature>
    {
        public void Configure(EntityTypeBuilder<ClientFeature> builder)
        {
            builder.ToTable("ClientFeatures");

            builder.HasKey(cf => cf.Id);

            builder.Property(cf => cf.Id)
                .ValueGeneratedOnAdd();

            builder.Property(cf => cf.ClientId)
                .IsRequired();

            builder.Property(cf => cf.FeatureId)
                .IsRequired();

            builder.Property(cf => cf.IsEnabled)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(cf => cf.CreatedAt)
                .IsRequired();

            builder.HasOne(cf => cf.Feature)
                .WithMany(f => f.ClientFeatures)
                .HasForeignKey(cf => cf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
