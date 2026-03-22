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
                .ForMember(dest => dest.AttendanceDate, 
                    opt => opt.MapFrom(src => DateOnly.FromDateTime(src.AttendanceDate)))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AttendanceDate,
                    opt => opt.MapFrom(src => new DateTime(src.AttendanceDate ?? new DateOnly(),
                                                    new TimeOnly())));

            CreateMap<Attendance, UpdateAttendanceCommand>()
                .ReverseMap();
        }
    }
}
