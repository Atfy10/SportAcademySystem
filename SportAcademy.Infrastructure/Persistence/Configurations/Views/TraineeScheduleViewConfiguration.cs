using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.TraineeViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views;

public class TraineeScheduleViewConfiguration
    : IEntityTypeConfiguration<TraineeScheduleView>
{
    public void Configure(EntityTypeBuilder<TraineeScheduleView> builder)
    {
        builder.ToView("vw_TraineeSchedule");

        builder.HasNoKey();

        builder.Property(x => x.TraineeGroupId);

        builder.Property(x => x.Day)
               .HasConversion<string>();

        builder.Property(x => x.StartTime);
    }
}