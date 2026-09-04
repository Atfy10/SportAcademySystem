using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities.Finance;

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
