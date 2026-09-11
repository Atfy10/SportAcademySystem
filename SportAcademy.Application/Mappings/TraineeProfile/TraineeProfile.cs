using AutoMapper;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Application.Mappings.TraineeProfile
{
    public class TraineeProfile : AutoMapper.Profile
    {
        public TraineeProfile()
        {
            CreateMap<string, Email>()
                .ConvertUsing(src => Email.Create(src));

            CreateMap<DateOnly, DateTime>()
                .ConvertUsing(d => d.ToDateTime(TimeOnly.MinValue));

            CreateMap<SportTrainee, string>()
                .ConvertUsing(st => st.Sport.Name);

            CreateMap<Trainee, TraineeCardDto>()
                .ForCtorParam("Code", o => o.MapFrom(s => s.TraineeCode.Value))
                .ForCtorParam("Age", o => o.MapFrom(s => GetAge(s)))
                .ForCtorParam("Email", o => o.MapFrom(s => s.Email.ToString()))
                .ForCtorParam("JoinDate", o => o.MapFrom(s => s.JoinDate.ToDateTime(TimeOnly.MinValue)))
                .ForCtorParam("IsSubscribed", o => o.MapFrom(s => s.SubscriptionDetails
                    .Any(sd => sd.Status == SubscriptionStatus.Active && !sd.IsDeleted)))
                .ForCtorParam("SportSkills", o => o.MapFrom(s => s.Sports
                    .Select(st => new TraineeSportSkillDto
                    {
                        SkillLevel = st.SkillLevel,
                        SportName = st.Sport.Name
                    }).ToList()))
                .ForCtorParam("CoachName", o => o.MapFrom(s =>
                    s.Enrollments.FirstOrDefault()!.TraineeGroup.Coach.Employee.FirstName +
                    " " + s.Enrollments.FirstOrDefault()!.TraineeGroup.Coach.Employee.LastName))
                .ForCtorParam("BranchName", o => o.MapFrom(s => s.Branch.Name ?? string.Empty))
                .ForMember(dest => dest.MedicalConditions, o => o.MapFrom(s =>
                    s.MedicalConditions.Select(mc => mc.Condition).ToList()))
                // Every caller (GetAll/Search/GetById TraineeQueryHandlers) fills this in after
                // mapping, from a separate attendance-summary query AutoMapper has no access to.
                .ForMember(dest => dest.AttendanceRate, o => o.Ignore())
                .ReverseMap();

            CreateMap<Trainee, TraineeDetailsDto>()
                .ForCtorParam("Code", o => o.MapFrom(s => s.TraineeCode.Value))
                .ForCtorParam("Email", o => o.MapFrom(s => s.Email.ToString()))
                .ForCtorParam("BranchName", o => o.MapFrom(s => s.Branch.Name ?? string.Empty))
                .ForCtorParam("Gender", o => o.MapFrom(s => s.Gender.ToString()))
                .ForCtorParam("Sports", o => o.MapFrom(s => s.Sports.Select(sport => sport.Sport.Name).ToList()))
                .ForCtorParam("IsSubscribed", o => o.MapFrom(s => s.SubscriptionDetails
                    .Any(sd => sd.Status == SubscriptionStatus.Active && !sd.IsDeleted)))
                .ForCtorParam("EnrollmentCount", o => o.MapFrom(s => s.Enrollments.Count))
                .ForCtorParam("JoinDate", o => o.MapFrom(s => s.JoinDate.ToDateTime(TimeOnly.MinValue)))
                .ForMember(dest => dest.MedicalConditions, o => o.MapFrom(s =>
                    s.MedicalConditions.Select(mc => mc.Condition).ToList()))
                .ForMember(dest => dest.AttendanceRate, o => o.Ignore());

            // CreateTraineeCommand/UpdateTraineePersonalCommand <-> Trainee are no longer
            // AutoMapper mappings — CreateTraineeCommandHandler/UpdateTraineePersonalCommandHandler
            // use Mappings/Manual/TraineeMapper.cs instead (nothing else referenced these maps).

            // Trainee <-> TraineeDto was dead (nothing calls Map<TraineeDto>/Map<Trainee> through
            // it - TraineeCardDto/TraineeDetailsDto/TraineeDropdownDto below are what's actually
            // used) and broken: TraineeDto.Sports is HashSet<SportDto> but the forward map built
            // HashSet<SportIdNameDto> instead, a type AutoMapper can't convert into it. Removed
            // rather than fixed for a type nothing exercises.

            CreateMap<Trainee, TraineeDropdownDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.SportSkills, opt => opt.MapFrom(src => src.Sports
                    .Select(s => new TraineeSportSkillItemDto(s.SportId, s.SkillLevel))));

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
