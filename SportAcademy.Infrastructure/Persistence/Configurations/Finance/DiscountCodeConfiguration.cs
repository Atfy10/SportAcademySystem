using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Finance
{
    public class DiscountCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
    {
        public void Configure(EntityTypeBuilder<DiscountCode> builder)
        {
            builder.ToTable("DiscountCodes");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Code).IsRequired().HasMaxLength(30);
            builder.Property(c => c.Description).HasMaxLength(200);
            builder.Property(c => c.PercentageOff).HasPrecision(5, 2);
            builder.Property(c => c.IsActive).HasDefaultValue(true);

            // An admin can't create two discount codes with the same (normalized) code within
            // their tenant.
            builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique();

            builder.HasMany(c => c.InvoiceLines)
                .WithOne(l => l.DiscountCode)
                .HasForeignKey(l => l.DiscountCodeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
