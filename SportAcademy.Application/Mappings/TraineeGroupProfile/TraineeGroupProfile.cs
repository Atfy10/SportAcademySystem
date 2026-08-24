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
            .ForMember(dest => dest.SportId,
                opt => opt.MapFrom(src => src.Coach.SportId))
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
            )
            .ForMember(dest => dest.Members,
                // Same enrollment set as TraineesCount above (no IsActive filter) - a
                // suspended/expired enrollment is still a trainee on this group's roster, just
                // with a status the SubscriptionStatus badge already conveys.
                opt => opt.MapFrom(src => src.Enrollments
                    .Select(e => new TraineeGroupMemberDto(
                        e.TraineeId,
                        e.Trainee.FirstName + " " + e.Trainee.LastName,
                        GetAge(e.Trainee),
                        DateOnly.FromDateTime(e.EnrollmentDate),
                        e.SubscriptionDetails.Status.ToString(),
                        e.SubscriptionDetails.EndDate
                    ))
                )
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

        // .ForCtorParam() here, not .ConstructUsing() - ConstructUsing opts a mapping out of
        // AutoMapper's LINQ expression-tree translation, which ProjectTo relies on to turn
        // this into a SQL projection (same root cause as the Coach dropdown fix). Plain
        // .ForMember() doesn't work either: both DTOs below are positional records with no
        // parameterless constructor, so AutoMapper must be told how to fill constructor
        // parameters explicitly via ForCtorParam.
        CreateMap<TraineeGroup, ListTraineeGroupDto>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("SportName", opt => opt.MapFrom(src => src.Coach.Sport.Name))
            .ForCtorParam("CoachName", opt => opt.MapFrom(src => src.Coach.Employee.FirstName))
            .ForCtorParam("BranchName", opt => opt.MapFrom(src => src.Branch.Name))
            .ForCtorParam("DurationInMinutes", opt => opt.MapFrom(src => src.DurationInMinutes))
            .ForCtorParam("TraineesCount", opt => opt.MapFrom(src => src.Enrollments.Count))
            .ForCtorParam("Schedules", opt => opt.MapFrom(src => src.GroupSchedules
                .Select(gs => new GroupScheduleItemDto
                {
                    DayOfWeek = gs.Day.ToString(),
                    StartTime = gs.StartTime.ToString("HH:mm:ss")
                }).ToList()));

        CreateMap<TraineeGroup, TraineeGroupDropdownDto>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name))
            .ForCtorParam("SportId", opt => opt.MapFrom(src => src.Coach.SportId))
            .ForCtorParam("BranchName", opt => opt.MapFrom(src => src.Branch.Name))
            .ForCtorParam("CoachName", opt => opt.MapFrom(src => src.Coach.Employee.FirstName))
            .ForCtorParam("SkillLevel", opt => opt.MapFrom(src => src.SkillLevel))
            .ForCtorParam("Gender", opt => opt.MapFrom(src => src.Gender));
    }

    private static int GetAge(Trainee trainee)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var birthDate = trainee.BirthDate;
        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
            age--;

        return age;
    }
}
