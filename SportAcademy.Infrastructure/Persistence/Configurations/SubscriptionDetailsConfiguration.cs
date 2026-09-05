using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

            builder.Property(sd => sd.GroupType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(TraineeGroupType.Public);

            // Stored as a short delimited string (e.g. "0,2,4") rather than a child table -
            // it's at most seven values, always read and written as a whole, and never queried
            // by individual day. The comparer is required for EF to track changes on a
            // collection-typed property; without it, edits to the list go undetected.
            builder.Property(sd => sd.TrainingDays)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue(new List<DayOfWeek>())
                .HasConversion(
                    days => string.Join(',', days.Select(d => (int)d)),
                    raw => raw.Length == 0
                        ? new List<DayOfWeek>()
                        : raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(v => (DayOfWeek)int.Parse(v))
                             .ToList(),
                    new ValueComparer<List<DayOfWeek>>(
                        (left, right) => left!.SequenceEqual(right!),
                        days => days.Aggregate(0, (hash, day) => HashCode.Combine(hash, day.GetHashCode())),
                        days => days.ToList()));

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
                       sd.GroupType,
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
