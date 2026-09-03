using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class GroupScheduleConfiguration : IEntityTypeConfiguration<GroupSchedule>
    {
        public void Configure(EntityTypeBuilder<GroupSchedule> builder)
        {
            builder.ToTable("GroupSchedules");

            builder.HasKey(gs => gs.Id);

            builder.Property(gs => gs.Day)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(gs => gs.StartTime)
                .IsRequired();

            // Relationships
            // TraineeGroup 1:M GroupSchedules is configured on the TraineeGroup side
            // (TraineeGroupConfiguration) - see the comment there for why it must not also be
            // configured here.

            // 1:M SessionOccurrences
            builder.HasMany(gs => gs.SessionOccurrences)
                .WithOne(so => so.GroupSchedule)
                .HasForeignKey(so => so.GroupScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
