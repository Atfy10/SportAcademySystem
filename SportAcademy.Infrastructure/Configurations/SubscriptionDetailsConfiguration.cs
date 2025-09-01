using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Configurations
{
    public class SubscriptionDetailsConfiguration : IEntityTypeConfiguration<SubscriptionDetails>
    {
        public void Configure(EntityTypeBuilder<SubscriptionDetails> builder)
        {
            //Table Name
            builder.ToTable("SubscriptionDetails");

            // PK
            builder.HasKey(sd => sd.Id);

            // Props
            builder.Property(sd => sd.PaymentNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(sd => sd.StartDate)
                .IsRequired();

            builder.Property(sd => sd.EndDate)
                .IsRequired();

            builder.Property(sd => sd.IsActive)
                .HasDefaultValue(true);

            // Relationships
            // 1:1 Payment
            builder.HasOne(sd => sd.Payment)
                   .WithOne(p => p.SubscriptionDetails)
                   .HasForeignKey<SubscriptionDetails>(sd => sd.PaymentNumber)
                   .HasPrincipalKey<Payment>(p => p.PaymentNumber)
                   .OnDelete(DeleteBehavior.Cascade);

            //  1:M  Trainee
            builder.HasOne(sd => sd.Trainee)
                   .WithMany(t => t.SubscriptionDetails)
                   .HasForeignKey(sd => sd.TraineeId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1:M SubscriptionType
            builder.HasOne(sd => sd.SubscriptionType)
                   .WithMany(st => st.SubscriptionDetails)
                   .HasForeignKey(sd => sd.SubscriptionTypeId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
