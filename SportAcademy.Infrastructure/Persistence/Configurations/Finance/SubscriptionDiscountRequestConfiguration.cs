using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Finance
{
    public class SubscriptionDiscountRequestConfiguration : IEntityTypeConfiguration<SubscriptionDiscountRequest>
    {
        public void Configure(EntityTypeBuilder<SubscriptionDiscountRequest> builder)
        {
            builder.ToTable("SubscriptionDiscountRequests");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.DiscountCode).IsRequired().HasMaxLength(30);
            builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.RejectionReason).HasMaxLength(500);

            // Same storage shape as SubscriptionDetails' equivalents (string enum, delimited
            // day list) - these are the values the subscription is created from at approval time.
            builder.Property(r => r.GroupType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(TraineeGroupType.Public);

            builder.Property(r => r.TrainingDays)
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

            builder.HasOne(r => r.Trainee)
                .WithMany()
                .HasForeignKey(r => r.TraineeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.SubscriptionType)
                .WithMany()
                .HasForeignKey(r => r.SubscriptionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Sport)
                .WithMany()
                .HasForeignKey(r => r.SportId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Branch)
                .WithMany()
                .HasForeignKey(r => r.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.PaymentType)
                .WithMany()
                .HasForeignKey(r => r.PaymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.CreatedSubscriptionDetails)
                .WithMany()
                .HasForeignKey(r => r.CreatedSubscriptionDetailsId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
