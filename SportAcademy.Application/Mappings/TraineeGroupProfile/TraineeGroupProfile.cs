using SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.TraineeGroupProfile
{
    public class TraineeGroupMappingProfile : AutoMapper.Profile
    {
        public TraineeGroupMappingProfile()
        {
            CreateMap<TraineeGroup, TraineeGroupDto>().ReverseMap();

            CreateMap<CreateTraineeGroupCommand, TraineeGroup>();

            CreateMap<UpdateTraineeGroupCommand, TraineeGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
