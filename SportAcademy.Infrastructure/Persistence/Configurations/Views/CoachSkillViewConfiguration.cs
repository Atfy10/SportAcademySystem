using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.CoachViews;

namespace SportAcademy.Infrastructure.Persistence.Views.CoachViews
{
    public class CoachSkillViewConfiguration : IEntityTypeConfiguration<CoachSkillView>
    {
        public void Configure(EntityTypeBuilder<CoachSkillView> builder)
        {
            builder.ToView("vw_CoachSkill");

            builder.HasNoKey();

            builder.Property(x => x.Id);

            builder.Property(x => x.SkillLevel)
                .HasConversion<string>();

            builder.Property(x => x.SportName);
        }
    }
}