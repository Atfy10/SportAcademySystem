using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class PaymentTypeConfiguration : IEntityTypeConfiguration<PaymentType>
    {
        public void Configure(EntityTypeBuilder<PaymentType> builder)
        {
            // Table Name
            builder.ToTable("PaymentTypes");

            // PK
            builder.HasKey(pt => pt.Id);

            // Props
            builder.Property(pt => pt.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(pt => pt.IsActive)
                .HasDefaultValue(true);

            builder.Property(pt => pt.IsDefault)
                .HasDefaultValue(false);

            // An admin can't create two payment types with the same name within their tenant.
            builder.HasIndex(pt => new { pt.TenantId, pt.Name })
                .IsUnique();

            // Relationships

            // 1:M Payments
            builder.HasMany(pt => pt.Payments)
                .WithOne(p => p.PaymentType)
                .HasForeignKey(p => p.PaymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
