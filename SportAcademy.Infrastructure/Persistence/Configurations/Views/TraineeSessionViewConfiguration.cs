using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.TraineeViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views;

public class TraineeSessionViewConfiguration
    : IEntityTypeConfiguration<TraineeSessionView>
{
    public void Configure(EntityTypeBuilder<TraineeSessionView> builder)
    {
        builder.ToView("vw_TraineeSession");

        builder.HasNoKey();

        builder.Property(x => x.Id);

        builder.Property(x => x.EnrollmentDate);

        builder.Property(x => x.ExpiryDate);

        builder.Property(x => x.SessionAllowed);

        builder.Property(x => x.SessionRemaining);

        builder.Property(x => x.TraineeGroupId);

        builder.Property(x => x.SkillLevel)
               .HasConversion<string>();

        builder.Property(x => x.MaximumCapacity);

        builder.Property(x => x.DurationInMinutes);

        builder.Property(x => x.Gender)
               .HasConversion<string>();

        builder.Property(x => x.BranchName);

        builder.Property(x => x.CoachName);
    }
}