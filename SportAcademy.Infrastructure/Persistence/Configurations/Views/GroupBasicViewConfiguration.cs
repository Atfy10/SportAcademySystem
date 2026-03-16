using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.GroupViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views
{
    public class GroupBasicViewConfiguration
    : IEntityTypeConfiguration<GroupBasicView>
    {
        public void Configure(EntityTypeBuilder<GroupBasicView> builder)
        {
            builder.ToView("vw_GroupBasic");

            builder.HasNoKey();

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
}