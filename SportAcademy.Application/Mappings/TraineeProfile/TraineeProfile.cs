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
                .ForAllMembers(opt => opt.Ignore());

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
                ))
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<CreateTraineeCommand, Trainee>()
                .ConstructUsing((src, _) =>
                {
                    var address = string.IsNullOrWhiteSpace(src.Street) && string.IsNullOrWhiteSpace(src.City)
                        ? Address.Create("Unknown", "Unknown")
                        : Address.Create(src.Street ?? "Unknown", src.City ?? "Unknown");

                    var email = Email.Create(src.Email);

                    var data = new PersonData(
                        src.FirstName,
                        src.LastName,
                        src.SSN,
                        email,
                        src.BirthDate,
                        src.Gender,
                        src.Nationality,
                        address,
                        src.PhoneNumber,
                        null);

                    var trainee = Trainee.Create(
                        data,
                        src.ParentNumber,
                        src.GuardianName,
                        src.BranchId,
                        src.NationalityCategoryId);

                    foreach (var sportId in src.SportIds)
                        trainee.AssignSport(sportId);

                    return trainee;
                })
                .ForAllMembers(opt => opt.Ignore());

            CreateMap<Trainee, CreateTraineeCommand>()
                .ForPath(dest => dest.Street, opt => opt.MapFrom(src => src.Address != null ? src.Address.Street : null))
                .ForPath(dest => dest.City, opt => opt.MapFrom(src => src.Address != null ? src.Address.City : null))
                .ForMember(dest => dest.SportIds,
                    opt => opt.MapFrom(src => src.Sports
                        .Select(st => st.SportId)
                        .ToHashSet()))
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Trainee, UpdateTraineePersonalCommand>()
                .ForMember(dest => dest.SportIds,
                    opt => opt.MapFrom(src => src.Sports.Select(st => st.SportId).ToList()));

            CreateMap<UpdateTraineePersonalCommand, Trainee>()
                .ForAllMembers(opts => opts.Ignore());

            CreateMap<Trainee, TraineeDto>()
                .ForMember(dest => dest.Sports, opt => opt.MapFrom(src => src.Sports.Select(st => new SportDto(
                    st.Sport.Id,
                    st.Sport.Name,
                    st.Sport.Description,
                    st.Sport.Category,
                    st.Sport.IsRequireHealthTest
                )).ToHashSet()));

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
