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
                .ReverseMap();

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
