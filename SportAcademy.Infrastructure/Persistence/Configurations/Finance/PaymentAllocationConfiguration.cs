using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Finance;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Finance
{
    public class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
    {
        public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
        {
            builder.ToTable("PaymentAllocations");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.PaymentNumber).IsRequired().HasMaxLength(50);
            builder.Property(a => a.Amount).HasPrecision(18, 3);

            builder.HasOne(a => a.Payment)
                   .WithMany(p => p.Allocations)
                   .HasForeignKey(a => a.PaymentNumber)
                   .HasPrincipalKey(p => p.PaymentNumber)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
