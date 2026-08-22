using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Mappings.FamilyProfile
{
    public class FamilyProfile : AutoMapper.Profile
    {
        public FamilyProfile()
        {
            // .ForCtorParam() here, not .ConstructUsing() - ConstructUsing opts a mapping out
            // of AutoMapper's LINQ expression-tree translation, which ProjectTo relies on to
            // turn this into a SQL projection (same root cause as the Coach dropdown fix). Plain
            // .ForMember() doesn't work either: FamilyDto is a positional record with no
            // parameterless constructor, so AutoMapper must be told how to fill constructor
            // parameters explicitly via ForCtorParam.
            CreateMap<Family, FamilyDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("Code", opt => opt.MapFrom(src => src.FamilyCode))
                .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name))
                .ForCtorParam("GuardianName", opt => opt.MapFrom(src => src.GuardianName))
                .ForCtorParam("GuardianPhone", opt => opt.MapFrom(src => src.GuardianPhone))
                .ForCtorParam("MemberCount", opt => opt.MapFrom(src => src.Members.Count));

            CreateMap<Family, FamilyDetailDto>()
                .ForCtorParam("Code", opt => opt.MapFrom(src => src.FamilyCode))
                .ForCtorParam("Members", opt => opt.MapFrom(src => src.Members));

            CreateMap<Trainee, FamilyMemberDto>()
                .ForCtorParam("Code", opt => opt.MapFrom(s => s.TraineeCode.Value))
                .ForCtorParam("Age", opt => opt.MapFrom(s => GetAge(s)))
                .ForCtorParam("IsSubscribed", opt => opt.MapFrom(s => s.SubscriptionDetails
                    .Any(sd => sd.Status == SubscriptionStatus.Active && !sd.IsDeleted)))
                .ForCtorParam("BranchName", opt => opt.MapFrom(s => s.Branch.Name ?? string.Empty));
        }

        private static int GetAge(Trainee trainee)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var birthDate = (DateOnly)trainee.BirthDate;
            var age = today.Year - birthDate.Year;

            if (birthDate > today.AddYears(-age))
                age--;

            return age;
        }
    }
}
