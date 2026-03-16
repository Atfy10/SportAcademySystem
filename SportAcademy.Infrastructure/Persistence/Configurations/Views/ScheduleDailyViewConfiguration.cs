using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.ScheduleViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views;

public class ScheduleDailyViewConfiguration
    : IEntityTypeConfiguration<ScheduleDailyView>
{
    public void Configure(EntityTypeBuilder<ScheduleDailyView> builder)
    {
        builder.ToView("vw_ScheduleDaily");

        builder.HasNoKey();

        builder.Property(x => x.TraineeGroupId);

        builder.Property(x => x.Day)
               .HasConversion<string>();

        builder.Property(x => x.StartTime);
    }
}