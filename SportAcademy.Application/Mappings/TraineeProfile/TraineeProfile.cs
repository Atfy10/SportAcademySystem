using AutoMapper;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Domain.Entities;
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
                .ConstructUsing(src => new TraineeCardDto(
                    src.Id,
                    src.TraineeCode.Value,
                    src.FirstName,
                    src.LastName,
                    GetAge(src),
                    src.Email.ToString(),
                    src.PhoneNumber,
                    src.JoinDate.ToDateTime(TimeOnly.MinValue),
                    src.IsSubscribed,
                    src.Sports.Select(st => new TraineeSportSkillDto
                    {
                        SkillLevel = st.SkillLevel,
                        SportName = st.Sport.Name
                    }).ToList(),
                    src.Enrollments.FirstOrDefault()!.TraineeGroup.Coach.Employee.FirstName +
                        " " + src.Enrollments.FirstOrDefault()!.TraineeGroup.Coach.Employee.LastName,
                    src.Branch.Name ?? string.Empty
                ))
                .ReverseMap();

            CreateMap<Trainee, TraineeDetailsDto>()
                .ConstructUsing(src => new TraineeDetailsDto(
                    src.Id,
                    src.TraineeCode.Value,
                    src.FirstName,
                    src.LastName,
                    src.Email.ToString(),
                    src.PhoneNumber,
                    src.ParentNumber,
                    src.GuardianName,
                    src.Branch.Name ?? string.Empty,
                    src.BirthDate,
                    src.Gender.ToString(),
                    src.Sports.Select(s => s.Sport.Name).ToList(),
                    src.IsSubscribed,
                    src.Enrollments.Count,
                    src.JoinDate.ToDateTime(TimeOnly.MinValue)
                ));

            CreateMap<CreateTraineeCommand, Trainee>()
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom((src, dest) =>
                    {
                        if (string.IsNullOrWhiteSpace(src.Street) && string.IsNullOrWhiteSpace(src.City))
                            return Address.Create("", "");
                        return Address.Create(src.Street ?? "", src.City ?? "");
                    }))
                .ForMember(dest => dest.Sports,
                    opt => opt.MapFrom(src => src.SportIds
                        .Select(id => new SportTrainee { SportId = id })
                        .ToList()))
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Trainee, CreateTraineeCommand>()
                .ForPath(dest => dest.Street, opt => opt.MapFrom(src => src.Address != null ? src.Address.Street : null))
                .ForPath(dest => dest.City, opt => opt.MapFrom(src => src.Address != null ? src.Address.City : null))
                .ForMember(dest => dest.SportIds,
                    opt => opt.MapFrom(src => src.Sports
                        .Select(st => st.SportId)
                        .ToHashSet()))
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Trainee, UpdateTraineePersonalCommand>();

            CreateMap<UpdateTraineePersonalCommand, Trainee>()
                .ForMember(src => src.Sports, opt => opt.Ignore())
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

            CreateMap<Trainee, TraineeDropdownDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

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
