using SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance;
using SportAcademy.Application.Commands.AttendanceCommands.UpdateAttendance;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Mappings.AttendanceProfile
{
    public class AttendanceProfile : AutoMapper.Profile
    {
        public AttendanceProfile()
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

            CreateMap<Attendance, CreateAttendanceCommand>()
                .ConstructUsing(src => new CreateAttendanceCommand(
                    DateOnly.FromDateTime(src.AttendanceDate),
                    src.AttendanceStatus,
                    src.CheckInTime,
                    src.CoachNote,
                    src.EnrollmentId,
                    src.SessionOccurrenceId
                ))
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<Attendance, UpdateAttendanceCommand>()
                .ReverseMap();
        }
    }
}
