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

            builder.HasData(
                new NationalityCategory(1, "AM", "American"),
                new NationalityCategory(2, "EU", "European"),
                new NationalityCategory(3, "AS", "Asian"),
                new NationalityCategory(4, "AF", "African"),
                new NationalityCategory(5, "AG", "Arab Gulf"),
                new NationalityCategory(6, "AR", "Arab"),
                new NationalityCategory(7, "OC", "Oceanian")
            );

            builder.HasIndex(nc => nc.Code)
                .IsUnique();

            builder.HasIndex(nc => nc.Name)
                .IsUnique();
        }
    }
}
