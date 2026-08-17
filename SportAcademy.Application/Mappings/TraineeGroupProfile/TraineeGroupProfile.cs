using Application.Common.Mapping.Converters;
using SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup;
using SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.GroupScheduleDtos;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.TraineeGroupProfile;

public class TraineeGroupMappingProfile : AutoMapper.Profile
{
    public TraineeGroupMappingProfile()
    {
        CreateMap<TraineeGroup, TraineeGroup>();

        CreateMap(typeof(PagedData<>), typeof(PagedData<>))
            .ConvertUsing(typeof(PagedDataConverter<,>));

        CreateMap<TraineeGroup, TraineeGroupDetailDto>()
            .ForMember(dest => dest.SportName,
                opt => opt.MapFrom(src => src.Coach.Sport.Name))
            .ForMember(dest => dest.CoachName,
                opt => opt.MapFrom(src => src.Coach.Employee.FirstName))
            .ForMember(dest => dest.BranchName,
                opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.Schedules,
                opt => opt.MapFrom(src => src.GroupSchedules
                        .Select(gs => new GroupScheduleDto
                        {
                            Id = gs.Id,
                            DayOfWeek = gs.Day,
                            StartTime = gs.StartTime,
                            EndTime = gs.StartTime.Add(TimeSpan.FromMinutes(src.DurationInMinutes))
                        }).ToList()
                )
            )
            .ForMember(dest => dest.TraineesCount,
                opt => opt.MapFrom(src => src.Enrollments.Count)
            );

        CreateMap<TraineeGroup, TraineeGroupCardDto>()
            .ForMember(dest => dest.SportName,
                opt => opt.MapFrom(src => src.Coach.Sport.Name))
            .ForMember(dest => dest.CoachName,
                opt => opt.MapFrom(src => src.Coach.Employee.FirstName))
            .ForMember(dest => dest.BranchName,
                opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.Schedules,
                opt => opt.MapFrom(src => src.GroupSchedules
                        .Select(gs => new GroupSchedulesTimesDto
                        {
                            DayOfWeek = gs.Day,
                            StartTime = gs.StartTime
                        }).ToList()
                )
            )
            .ForMember(dest => dest.TraineesCount,
                opt => opt.MapFrom(src => src.Enrollments.Count)
            );

        // TraineeGroup <-> TraineeGroupDto and UpdateTraineeGroupCommand -> TraineeGroup are no
        // longer AutoMapper mappings - UpdateTraineeGroupCommandHandler uses
        // Mappings/Manual/TraineeGroupMapper.cs instead (it must skip BranchId during update,
        // which a declarative map can't express without also breaking create).

        CreateMap<CreateTraineeGroupCommand, TraineeGroup>();

        CreateMap<TraineeGroup, ListTraineeGroupDto>()
            .ConstructUsing(src => new ListTraineeGroupDto(
                src.Id,
                src.Coach.Sport.Name,
                src.Coach.Employee.FirstName,
                src.Branch.Name,
                src.DurationInMinutes,
                src.Enrollments.Count,
                src.GroupSchedules
                    .Select(gs => new GroupScheduleItemDto
                    {
                        DayOfWeek = gs.Day.ToString(),
                        StartTime = gs.StartTime.ToString("HH:mm:ss")
                    }).ToList()))
            .ForMember(dest => dest.SportName, opt => opt.Ignore())
            .ForMember(dest => dest.CoachName, opt => opt.Ignore())
            .ForMember(dest => dest.BranchName, opt => opt.Ignore())
            .ForMember(dest => dest.DurationInMinutes, opt => opt.Ignore())
            .ForMember(dest => dest.TraineesCount, opt => opt.Ignore())
            .ForMember(dest => dest.Schedules, opt => opt.Ignore());

        CreateMap<TraineeGroup, TraineeGroupDropdownDto>()
            .ConstructUsing(src => new TraineeGroupDropdownDto(src.Id, src.Name));
    }
}
