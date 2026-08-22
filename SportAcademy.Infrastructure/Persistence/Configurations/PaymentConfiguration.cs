using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            //Table Name 
            builder.ToTable("Payments");
            //PK
            builder.HasKey(p => p.PaymentNumber);

            //Props
            builder.Property(p => p.PaymentNumber)
                   .IsRequired()
                   .HasMaxLength(50); 

            builder.Property(p => p.Method)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(p => p.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(p => p.Currency)
                   .IsRequired()
                   .HasMaxLength(3);

            builder.Property(p => p.Reference).HasMaxLength(100);
            builder.Property(p => p.Notes).HasMaxLength(1000);

            // KWD (and several other Gulf currencies) has 3 decimal places - decimal(18,2)
            // would silently truncate fils.
            builder.Property(p => p.Amount).HasPrecision(18, 3);
            builder.Property(p => p.RefundedAmount).HasPrecision(18, 3);

            builder.Property(p => p.PaidDate)
                   .IsRequired();

            // Relationships

            // 1:M Branch
            builder.HasOne(p => p.Branch)
                   .WithMany(b => b.Payments)
                   .HasForeignKey(p => p.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);

            // A payment settles whatever it's allocated to (see PaymentAllocation) rather than
            // one fixed SubscriptionDetails - that 1:1 shape couldn't express partial payments,
            // instalments, or one payment covering several invoices.
        }
    }
}
