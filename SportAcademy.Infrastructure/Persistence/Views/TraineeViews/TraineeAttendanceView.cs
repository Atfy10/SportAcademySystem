using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.Views.Interfaces;

namespace SportAcademy.Infrastructure.Persistence.Views.TraineeViews
{
    public class TraineeAttendanceView : IModelView
    {
        public DateTime StartDateTime { get; set; }

        public SessionStatus Status { get; set; }

        public DateOnly AttendanceDate { get; set; }

        public AttendanceStatus AttendanceStatus { get; set; }

        public TimeOnly? CheckInTime { get; set; }

        public string? CoachNote { get; set; }
    }
}
