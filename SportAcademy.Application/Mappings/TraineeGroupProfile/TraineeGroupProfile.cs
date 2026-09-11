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
            .ForMember(dest => dest.SkillLevel,
                opt => opt.MapFrom(src => src.SkillLevel.ToString()))
            .ForMember(dest => dest.Gender,
                opt => opt.MapFrom(src => src.Gender.ToString()))
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Schedules,
                opt => opt.MapFrom(src => src.GroupSchedules
                        .Select(gs => new GroupScheduleDto
                        {
                            Id = gs.Id,
                            DayOfWeek = gs.Day.ToString(),
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
                            DayOfWeek = gs.Day.ToString(),
                            StartTime = gs.StartTime
                        }).ToList()
                )
            )
            .ForMember(dest => dest.TraineesCount,
                opt => opt.MapFrom(src => src.Enrollments.Count)
            )
            .ForMember(dest => dest.MaximumCapacity,
                opt => opt.MapFrom(src => src.MaximumCapacity)
            )
            .ForMember(dest => dest.SkillLevel,
                opt => opt.MapFrom(src => src.SkillLevel.ToString())
            );

        // TraineeGroup <-> TraineeGroupDto and UpdateTraineeGroupCommand -> TraineeGroup are no
        // longer AutoMapper mappings - UpdateTraineeGroupCommandHandler uses
        // Mappings/Manual/TraineeGroupMapper.cs instead (it must skip BranchId during update,
        // which a declarative map can't express without also breaking create).

        // Everything below is entity-owned (audit/tenant/soft-delete) or a navigation/derived
        // collection the handler populates separately (e.g. GroupSchedules from
        // command.Schedules) - none of it comes from the command itself.
        CreateMap<CreateTraineeGroupCommand, TraineeGroup>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.InactiveReason, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.Tenant, opt => opt.Ignore())
            .ForMember(dest => dest.Branch, opt => opt.Ignore())
            .ForMember(dest => dest.Coach, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore())
            .ForMember(dest => dest.GroupSchedules, opt => opt.Ignore())
            .ForMember(dest => dest.Translations, opt => opt.Ignore());

        // .ForCtorParam() here, not .ConstructUsing() - ConstructUsing opts a mapping out of
        // AutoMapper's LINQ expression-tree translation, which ProjectTo relies on to turn
        // this into a SQL projection (same root cause as the Coach dropdown fix). Plain
        // .ForMember() doesn't work either: both DTOs below are positional records with no
        // parameterless constructor, so AutoMapper must be told how to fill constructor
        // parameters explicitly via ForCtorParam.
        CreateMap<TraineeGroup, ListTraineeGroupDto>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name))
            .ForCtorParam("SportName", opt => opt.MapFrom(src => src.Coach.Sport.Name))
            .ForCtorParam("CoachName", opt => opt.MapFrom(src => src.Coach.Employee.FirstName))
            .ForCtorParam("BranchName", opt => opt.MapFrom(src => src.Branch.Name))
            .ForCtorParam("DurationInMinutes", opt => opt.MapFrom(src => src.DurationInMinutes))
            .ForCtorParam("TraineesCount", opt => opt.MapFrom(src => src.Enrollments.Count))
            .ForCtorParam("MaximumCapacity", opt => opt.MapFrom(src => src.MaximumCapacity))
            .ForCtorParam("SkillLevel", opt => opt.MapFrom(src => src.SkillLevel.ToString()))
            .ForCtorParam("IsActive", opt => opt.MapFrom(src => src.IsActive))
            .ForCtorParam("InactiveReason", opt => opt.MapFrom(src => src.InactiveReason))
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
            .ForCtorParam("Gender", opt => opt.MapFrom(src => src.Gender))
            .ForCtorParam("Type", opt => opt.MapFrom(src => src.Type))
            // TrainingDays has no matching member on TraineeGroup - it's derived from the
            // group's schedule. Every constructor parameter must be satisfiable or AutoMapper
            // discards the constructor entirely and then reports the *first* ForCtorParam as
            // having no matching constructor, which is a thoroughly misleading error.
            .ForCtorParam("TrainingDays", opt => opt.MapFrom(src => src.GroupSchedules
                .Select(gs => gs.Day)
                .Distinct()
                .OrderBy(d => d)
                .Select(d => d.ToString())
                .ToList()));
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
