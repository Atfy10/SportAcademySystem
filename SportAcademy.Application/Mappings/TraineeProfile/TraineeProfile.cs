using AutoMapper;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.TraineeProfile
{
    public class TraineeProfile : AutoMapper.Profile
    {
        public TraineeProfile()
        {
            CreateMap<DateOnly, DateTime>()
                .ConvertUsing(d => d.ToDateTime(TimeOnly.MinValue));

            CreateMap<Trainee, TraineeCardDto>()
                .ConstructUsing(src => new TraineeCardDto(
                    src.Id,
                    src.FirstName,
                    src.LastName,
                    GetAge(src),
                    src.Email.ToString(),
                    src.PhoneNumber,
                    src.JoinDate.ToDateTime(TimeOnly.MinValue),
                    src.IsSubscribed,
                    src.Sports.FirstOrDefault()!.Sport.Name ?? string.Empty,
                    src.Enrollments.FirstOrDefault()!.TraineeGroup.Coach.Employee.FirstName +
                    " " + src.Enrollments.FirstOrDefault()!.TraineeGroup.Coach.Employee.LastName,
                    src.Sports.FirstOrDefault()!.SkillLevel.ToString() ?? string.Empty,
                    src.SubscriptionDetails
                        .FirstOrDefault()!.SportPrice.Branch.Name ?? string.Empty
                ))
                .ReverseMap();

            CreateMap<Trainee, CreateTraineeCommand>()
                .ForMember(dest => dest.Sports, opt => opt.MapFrom(src => src.Sports.Select(st => new SportIdNameDto(st.SportId,
                    st.Sport.Name
                )).ToHashSet()))
                .ReverseMap()
                .ForMember(dest => dest.Sports, opt => opt.MapFrom(src => src.Sports.Select(s => new SportTrainee
                {
                    SportId = s.Id
                }).ToList()))
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Trainee, UpdateTraineePersonalCommand>();

            CreateMap<UpdateTraineePersonalCommand, Trainee>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Trainee, TraineeDto>()
                .ForMember(dest => dest.Sports, opt => opt.MapFrom(src => src.Sports.Select(st => new SportIdNameDto(st.Sport.Id,
                    st.Sport.Name
                )).ToHashSet()))
                .ReverseMap()
                .ForMember(dest => dest.Sports, opt => opt.MapFrom(src => src.Sports.Select(s => new SportTrainee
                {
                    SportId = s.Id
                }).ToList()));
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

        private static DateTime GetDate(Trainee trainee)
        {
            return new DateTime(trainee.JoinDate, new TimeOnly());
        }
    }
}
