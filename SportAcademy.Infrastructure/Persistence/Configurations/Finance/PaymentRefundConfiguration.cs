using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Finance
{
    public class PaymentRefundConfiguration : IEntityTypeConfiguration<PaymentRefund>
    {
        public void Configure(EntityTypeBuilder<PaymentRefund> builder)
        {
            builder.ToTable("PaymentRefunds");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.PaymentNumber).IsRequired().HasMaxLength(50);
            builder.Property(r => r.Kind).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.Amount).HasPrecision(18, 3);
            builder.Property(r => r.Reason).IsRequired().HasMaxLength(500);

            // Reports bucket refunds by the month the money actually went back out.
            builder.HasIndex(r => r.RefundedAt);

            builder.HasOne(r => r.Payment)
                   .WithMany(p => p.Refunds)
                   .HasForeignKey(r => r.PaymentNumber)
                   .HasPrincipalKey(p => p.PaymentNumber)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
