using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.TraineeViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views;

public class TraineeSubscriptionViewConfiguration
    : IEntityTypeConfiguration<TraineeSubscriptionView>
{
    public void Configure(EntityTypeBuilder<TraineeSubscriptionView> builder)
    {
        builder.ToView("vw_TraineeSubscription");

        builder.HasNoKey();

        builder.Property(x => x.Id);
        builder.Property(x => x.FirstName);
        builder.Property(x => x.IsSubscribed);
        builder.Property(x => x.GuardianName);
        builder.Property(x => x.StartDate);
        builder.Property(x => x.EndDate);
        builder.Property(x => x.PaymentNumber);
        builder.Property(x => x.SubscriptionTypeName);
        builder.Property(x => x.SportName);
        builder.Property(x => x.BranchName);
    }
}