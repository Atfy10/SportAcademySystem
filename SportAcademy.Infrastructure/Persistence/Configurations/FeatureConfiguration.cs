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

            builder.Property(f => f.IsEnabled)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(f => f.CreatedAt)
                .IsRequired();

            builder.HasMany(f => f.ClientFeatures)
                .WithOne(cf => cf.Feature)
                .HasForeignKey(cf => cf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(
                new Feature
                {
                    Id = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890"),
                    Name = "Inventory",
                    DisplayName = "Inventory Module",
                    IsEnabled = false,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Feature
                {
                    Id = Guid.Parse("B2C3D4E5-F6A7-8901-BCDE-F12345678901"),
                    Name = "Financials",
                    DisplayName = "Financials Module",
                    IsEnabled = false,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Feature
                {
                    Id = Guid.Parse("C3D4E5F6-A7B8-9012-CDEF-123456789012"),
                    Name = "AttendanceV2",
                    DisplayName = "Attendance V2 Module",
                    IsEnabled = false,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Feature
                {
                    Id = Guid.Parse("D4E5F6A7-B8C9-0123-DEF1-234567890123"),
                    Name = "Reports",
                    DisplayName = "Reports Module",
                    IsEnabled = false,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Feature
                {
                    Id = Guid.Parse("E5F6A7B8-C9D0-1234-EF12-345678901234"),
                    Name = "Notifications",
                    DisplayName = "Notifications Module",
                    IsEnabled = false,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
