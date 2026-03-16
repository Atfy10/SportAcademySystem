using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.CoachViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views;

public class CoachScheduleViewConfiguration
    : IEntityTypeConfiguration<CoachScheduleView>
{
    public void Configure(EntityTypeBuilder<CoachScheduleView> builder)
    {
        builder.ToView("vw_CoachSchedule");

        builder.HasNoKey();

        builder.Property(x => x.TraineeGroupId);

        builder.Property(x => x.SkillLevel)
            .HasConversion<string>();

        builder.Property(x => x.MaximumCapacity);

        builder.Property(x => x.DurationInMinutes);

        builder.Property(x => x.Gender)
            .HasConversion<string>();

        builder.Property(x => x.BranchName)
               .HasMaxLength(200);

        builder.Property(x => x.CoachName)
               .HasMaxLength(200);
    }
}