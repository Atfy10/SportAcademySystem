using AutoMapper;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Mappings
{
    public class SportTraineeMappingProfile : AutoMapper.Profile
    {
        public SportTraineeMappingProfile()
        {
            CreateMap<SportTrainee, SportTraineeDto>()
                .ConstructUsing(src => new SportTraineeDto
                {
                    SportId = src.SportId,
                    TraineeId = src.TraineeId,
                    SkillLevel = src.SkillLevel.ToString(),
                    SportName = src.Sport == null ? string.Empty : src.Sport.Name,
                    TraineeName = src.Trainee == null ? string.Empty : src.Trainee.FirstName + " " + src.Trainee.LastName
                });
        }
    }
}
