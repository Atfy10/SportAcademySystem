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
            // .ForCtorParam() here, not .ConstructUsing() - ConstructUsing opts a mapping out of
            // AutoMapper's LINQ expression-tree translation, which ProjectTo relies on to turn
            // this into a SQL projection (same root cause as the earlier Coach dropdown fix).
            // Plain .ForMember() doesn't work either: these are positional records with no
            // parameterless constructor, so AutoMapper must be told how to fill constructor
            // parameters explicitly via ForCtorParam.
            CreateMap<Attendance, AttendanceDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("AttendanceDate", opt => opt.MapFrom(src => src.AttendanceDate))
                .ForCtorParam("AttendanceStatus", opt => opt.MapFrom(src => src.AttendanceStatus.ToString()))
                .ForCtorParam("CheckInTime", opt => opt.MapFrom(src => src.CheckInTime.ToString("HH:mm:ss")))
                .ForCtorParam("CoachNote", opt => opt.MapFrom(src => src.CoachNote ?? string.Empty))
                .ForCtorParam("EnrollmentId", opt => opt.MapFrom(src => src.EnrollmentId))
                .ForCtorParam("SessionOccurrenceId", opt => opt.MapFrom(src => src.SessionOccurrenceId));

            CreateMap<Attendance, AttendanceRecordDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("TraineeId", opt => opt.MapFrom(src => src.Enrollment!.TraineeId))
                .ForCtorParam("TraineeName", opt => opt.MapFrom(src => src.Enrollment!.Trainee.FirstName + " " + src.Enrollment.Trainee.LastName))
                .ForCtorParam("CheckInTime", opt => opt.MapFrom(src => src.CheckInTime.ToString("HH:mm:ss")))
                .ForCtorParam("Status", opt => opt.MapFrom(src => src.AttendanceStatus.ToString()));

            // CreateAttendanceCommand <-> Attendance is no longer an AutoMapper mapping:
            // CreateAttendanceCommandHandler builds/updates the entity manually (see
            // Mappings/Manual — it needs to resolve EnrollmentId from TraineeId first,
            // which a declarative map can't express).

            CreateMap<Attendance, UpdateAttendanceCommand>()
                .ReverseMap();
        }
    }
}
