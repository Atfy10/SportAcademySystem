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
                new NationalityCategory { Id = 1, Code = "AM", Name = "American" },
                new NationalityCategory { Id = 2, Code = "EU", Name = "European" },
                new NationalityCategory { Id = 3, Code = "AS", Name = "Asian" },
                new NationalityCategory { Id = 4, Code = "AF", Name = "African" },
                new NationalityCategory { Id = 5, Code = "AG", Name = "Arab Gulf" },
                new NationalityCategory { Id = 6, Code = "AR", Name = "Arab" },
                new NationalityCategory { Id = 7, Code = "OC", Name = "Oceanian" }
            );

            builder.HasIndex(nc => nc.Code)
                .IsUnique();

            builder.HasIndex(nc => nc.Name)
                .IsUnique();
        }
    }
}
