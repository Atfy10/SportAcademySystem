using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.Persistence.Configurations
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
            builder.Property(sd => sd.StartDate)
                .IsRequired();

            builder.Property(sd => sd.EndDate)
                .IsRequired();

            builder.Property(sd => sd.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(SubscriptionStatus.Active);

            // Relationships
            // Billed via an InvoiceLine (see Finance.InvoiceLine.SubscriptionDetailsId) rather
            // than a fixed 1:1 Payment - money now lives in the Finance.* model.

            //  1:M  Trainee
            builder.HasOne(sd => sd.Trainee)
                   .WithMany(t => t.SubscriptionDetails)
                   .HasForeignKey(sd => sd.TraineeId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 1:M SportPrice
            builder.HasOne(sd => sd.SportPrice)
                   .WithMany(sp => sp.SubscriptionsDetails)
                   .HasForeignKey(sd => new {
                       sd.SportId,
                       sd.BranchId,
                       sd.SubscriptionTypeId,
                   })
                   .OnDelete(DeleteBehavior.Restrict);

            // 1:1 Enrollment
            builder.HasOne(sd => sd.Enrollment)
                   .WithOne(e => e.SubscriptionDetails)
                   .HasForeignKey<Enrollment>(e => e.SubscriptionDetailsId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
