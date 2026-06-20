using AutoMapper;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Mappings
{
    public class AttendanceMappingProfile : AutoMapper.Profile
    {
        public AttendanceMappingProfile()
        {
            CreateMap<Attendance, AttendanceDto>()
                .ConstructUsing(src => new AttendanceDto(
                    src.Id,
                    src.AttendanceDate,
                    src.AttendanceStatus.ToString(),
                    src.CheckInTime.ToString("HH:mm:ss"),
                    src.CoachNote ?? string.Empty,
                    src.EnrollmentId,
                    src.SessionOccurrenceId
                ));

            CreateMap<Attendance, AttendanceRecordDto>()
                .ConstructUsing(src => new AttendanceRecordDto(
                    src.Id,
                    src.Enrollment!.TraineeId,
                    $"{src.Enrollment.Trainee.FirstName} {src.Enrollment.Trainee.LastName}",
                    src.CheckInTime.ToString("HH:mm:ss"),
                    src.AttendanceStatus.ToString()
                ));
        }
    }
}
