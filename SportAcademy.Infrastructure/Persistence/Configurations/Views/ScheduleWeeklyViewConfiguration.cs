using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.ScheduleViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views;

public class ScheduleWeeklyViewConfiguration
    : IEntityTypeConfiguration<ScheduleWeeklyView>
{
    public void Configure(EntityTypeBuilder<ScheduleWeeklyView> builder)
    {
        builder.ToView("vw_ScheduleWeekly");

        builder.HasNoKey();

        builder.Property(x => x.TraineeGroupId);

        builder.Property(x => x.Day)
               .HasConversion<string>();

        builder.Property(x => x.StartTime);
    }
}