using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Infrastructure.Persistence.Views.TraineeViews;

namespace SportAcademy.Infrastructure.Persistence.Configurations.Views;

public class TraineeAttendanceViewConfiguration
    : IEntityTypeConfiguration<TraineeAttendanceView>
{
    public void Configure(EntityTypeBuilder<TraineeAttendanceView> builder)
    {
        builder.ToView("vw_TraineeAttendance");

        builder.HasNoKey();

        builder.Property(x => x.StartDateTime);

        builder.Property(x => x.Status)
               .HasConversion<string>();

        builder.Property(x => x.AttendanceDate);

        builder.Property(x => x.AttendanceStatus)
               .HasConversion<string>();

        builder.Property(x => x.CheckInTime);

        builder.Property(x => x.CoachNote);
    }
}