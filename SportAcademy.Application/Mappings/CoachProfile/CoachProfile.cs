using SportAcademy.Application.Commands.CoachCommands.CreateCoach;
using SportAcademy.Application.Commands.CoachCommands.CreateCoachWithEmployee;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.CoachProfile
{
    public class CoachProfile : AutoMapper.Profile
    {
        public CoachProfile()
        {
            // .ForCtorParam() here, not .ConstructUsing() - see note on CoachDropdownItemDto
            // below; this is the exact same InvalidCastException-on-ProjectTo root cause. Plain
            // .ForMember() doesn't work either: CoachCardDto is a positional record with no
            // parameterless constructor, so AutoMapper must be told how to fill constructor
            // parameters explicitly via ForCtorParam.
            CreateMap<Coach, CoachCardDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.EmployeeId))
                .ForCtorParam("FirstName", opt => opt.MapFrom(src => src.Employee.FirstName))
                .ForCtorParam("LastName", opt => opt.MapFrom(src => src.Employee.LastName))
                .ForCtorParam("Position", opt => opt.MapFrom(src => src.Employee.Position.ToString()))
                .ForCtorParam("BranchName", opt => opt.MapFrom(src => src.Employee.Branch.Name))
                .ForCtorParam("Email", opt => opt.MapFrom(src => src.Employee.Email.ToString()))
                .ForCtorParam("IsWork", opt => opt.MapFrom(src => src.Employee.IsWork))
                .ForCtorParam("PhoneNumber", opt => opt.MapFrom(src => src.Employee.PhoneNumber))
                .ForCtorParam("Address", opt => opt.MapFrom(src => src.Employee.Address.ToString()))
                .ForCtorParam("HireDate", opt => opt.MapFrom(src => src.Employee.HireDate))
                .ForCtorParam("TotalTrainees", opt => opt.MapFrom(src => src.TraineeGroups
                    .SelectMany(tg => tg.Enrollments)
                    .Count(e => e.IsActive && !e.IsDeleted)))
                .ForCtorParam("SkillLevel", opt => opt.MapFrom(src => src.SkillLevel))
                .ForCtorParam("SportName", opt => opt.MapFrom(src => src.Sport.Name))
                .ForCtorParam("ImageUrl", opt => opt.MapFrom(src => src.Employee.ImageUrl))
                .ReverseMap();

            // New coaches default to a Rate of 3 (not 0 - CLR default - which would read as
            // "worst possible rating" rather than "not yet rated"). Adjustable afterward via
            // RateCoachCommand / PATCH api/coach/{id}/rate.
            CreateMap<CreateCoachCommand, Coach>()
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => 3))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.Tenant, opt => opt.Ignore())
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.Sport, opt => opt.Ignore())
                .ForMember(dest => dest.TraineeGroups, opt => opt.Ignore());

            CreateMap<CreateCoachWithEmployeeCommand, Coach>()
                .ForMember(
                    dest => dest.Employee,
                    opt => opt.Ignore()
                )
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => 3))
                .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.Tenant, opt => opt.Ignore())
                .ForMember(dest => dest.Sport, opt => opt.Ignore())
                .ForMember(dest => dest.TraineeGroups, opt => opt.Ignore());

            // CoachSummaryDto and the Coach -> CoachDropdownItemDto direction below are both
            // dead - nothing calls Map<CoachSummaryDto>/Map<CoachDropdownItemDto> or
            // ProjectTo<CoachDropdownItemDto> anywhere. The real dropdown path is
            // CoachRepository.GetAllForDropdownAsync, which projects through
            // CoachProjections.ToDropdownDto + a manual BranchIds backfill instead. Removed
            // rather than "fixed" - CoachSummaryDto.BirthDate in particular has no source
            // anywhere in the Coach/Employee graph (Employee has no BirthDate property), so
            // inventing a mapping for it would be fabricating behavior nothing exercises.

            // .ForCtorParam() here, not .ConstructUsing() - ConstructUsing satisfies every
            // constructor argument at runtime (this map is genuinely live, via
            // GetCoachByIdQueryHandler's plain Map<CoachDetailsDto> call) but AutoMapper's
            // config validator can't see through a ConstructUsing delegate to know that, so it
            // flags every one of this record's positional parameters as "unmapped" - same root
            // cause already documented below for CoachDropdownItemDto. ForCtorParam keeps the
            // exact same value expressions while giving the validator per-parameter visibility.
            CreateMap<Coach, CoachDetailsDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.EmployeeId))
                .ForCtorParam("FirstName", opt => opt.MapFrom(src => src.Employee.FirstName))
                .ForCtorParam("LastName", opt => opt.MapFrom(src => src.Employee.LastName))
                .ForCtorParam("Email", opt => opt.MapFrom(src => src.Employee.Email.ToString()))
                .ForCtorParam("PhoneNumber", opt => opt.MapFrom(src => src.Employee.PhoneNumber))
                .ForCtorParam("BranchName", opt => opt.MapFrom(src => src.Employee.Branch.Name))
                .ForCtorParam("SportName", opt => opt.MapFrom(src => src.Sport.Name))
                .ForCtorParam("SkillLevel", opt => opt.MapFrom(src => src.SkillLevel.ToString()))
                .ForCtorParam("Certifications", opt => opt.MapFrom(src => (string[]?)null)) // not implemented yet
                .ForCtorParam("TotalTrainees", opt => opt.MapFrom(src => src.TraineeGroups
                    .SelectMany(tg => tg.Enrollments)
                    .Count(e => e.IsActive && !e.IsDeleted)))
                .ForCtorParam("HireDate", opt => opt.MapFrom(src => src.Employee.HireDate))
                .ForCtorParam("IsWork", opt => opt.MapFrom(src => src.Employee.IsWork))
                .ForCtorParam("Rating", opt => opt.MapFrom(src => src.Rate))
                .ForCtorParam("ImageUrl", opt => opt.MapFrom(src => src.Employee.ImageUrl));
        }
    }
}
