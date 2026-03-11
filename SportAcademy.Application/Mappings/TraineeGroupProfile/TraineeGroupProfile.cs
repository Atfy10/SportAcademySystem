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

        CreateMap<TraineeGroup, TraineeGroupDto>()
            .ConstructUsing(src => new TraineeGroupDto(
                src.Id,
                src.SkillLevel,
                src.MaximumCapacity,
                src.DurationInMinutes,
                src.Gender,
                src.BranchId,
                src.CoachId
            ))
            .ReverseMap();

        CreateMap<CreateTraineeGroupCommand, TraineeGroup>();

        CreateMap<UpdateTraineeGroupCommand, TraineeGroup>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<TraineeGroup, ListTraineeGroupDto>()
            .ForMember(dest => dest.SportName,
                opt => opt.MapFrom(src => src.Coach.Sport.Name))
            .ForMember(dest => dest.CoachName,
                opt => opt.MapFrom(src => src.Coach.Employee.FirstName))
            .ForMember(dest => dest.BranchName,
                opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.StartTime,
                opt => opt.MapFrom(src => src.GroupSchedules
                                            .Select(gs => gs.StartTime)
                                            .FirstOrDefault())
            );
    }
}
