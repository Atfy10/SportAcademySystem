using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class NationalityCategoryConfiguration : IEntityTypeConfiguration<NationalityCategory>
    {
        public void Configure(EntityTypeBuilder<NationalityCategory> builder)
        {
            builder.ToTable("NationalityCategories");

            builder.HasKey(nc => nc.Id);

            builder.Property(nc => nc.Code)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(nc => nc.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasMany(nc => nc.Trainees)
                .WithOne(t => t.NationalityCategory)
                .HasForeignKey(t => t.NationalityCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(nc => nc.Code)
                .IsUnique();

            builder.HasIndex(nc => nc.Name)
                .IsUnique();
        }
    }
}
