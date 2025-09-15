using AutoMapper;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.TraineeProfile
{
    public class TraineeProfile : AutoMapper.Profile
    {
        public TraineeProfile()
        {
            CreateMap<Trainee, CreateTraineeCommand>().ReverseMap();
            CreateMap<Trainee, UpdateTraineeBasicsCommand>().ReverseMap();
        }
    }
}
