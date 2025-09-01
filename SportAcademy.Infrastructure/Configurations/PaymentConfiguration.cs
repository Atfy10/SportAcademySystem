using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Configurations
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

            builder.Property(p => p.PaidDate)
                   .IsRequired();

            // Relationships

            // 1:M Branch
            builder.HasOne(p => p.Branch)
                   .WithMany(b => b.Payments)
                   .HasForeignKey(p => p.BranchId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1:1 SubscriptionDetails
            builder.HasOne(p => p.SubscriptionDetails)
                   .WithOne(sd => sd.Payment)
                   .HasForeignKey<SubscriptionDetails>(sd => sd.PaymentNumber)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
