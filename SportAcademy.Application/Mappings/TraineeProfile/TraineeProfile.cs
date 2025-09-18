using AutoMapper;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.TraineeProfile
{
    public class TraineeProfile : AutoMapper.Profile
    {
        public TraineeProfile()
        {
            CreateMap<Trainee, CreateTraineeCommand>()
                .ForMember(dest => dest.Sports, opt=>opt.MapFrom(src => src.Sports.Select(st => new SportDto
                {
                    Id = st.SportId,
                    Name = st.Sport.Name
                }).ToHashSet()))
                .ReverseMap()
                .ForMember(dest => dest.Sports,
                opt => opt.MapFrom(src => src.Sports.Select(s => new SportTrainee
                {
                    SportId = s.Id
                }).ToList()))
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Trainee, UpdateTraineePersonalCommand>().ReverseMap();
        }
    }
}
