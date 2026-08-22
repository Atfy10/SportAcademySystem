using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Finance
{
    public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
    {
        public void Configure(EntityTypeBuilder<InvoiceLine> builder)
        {
            builder.ToTable("InvoiceLines");
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(l => l.Description).IsRequired().HasMaxLength(200);
            builder.Property(l => l.UnitPrice).HasPrecision(18, 3);
            builder.Property(l => l.DiscountAmount).HasPrecision(18, 3);
            builder.Property(l => l.LineTotal).HasPrecision(18, 3);

            builder.HasOne(l => l.SubscriptionDetails)
                   .WithMany(sd => sd.InvoiceLines)
                   .HasForeignKey(l => l.SubscriptionDetailsId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
