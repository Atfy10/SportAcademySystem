using SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup;
using SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.TraineeGroupProfile;

public class TraineeGroupMappingProfile : AutoMapper.Profile
{
    public TraineeGroupMappingProfile()
    {
        CreateMap<TraineeGroup, TraineeGroupDto>().ReverseMap();

        CreateMap<CreateTraineeGroupCommand, TraineeGroup>();

        CreateMap<UpdateTraineeGroupCommand, TraineeGroup>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<TraineeGroup, ListTraineeGroupDto>()
            .ForMember(dest => dest.SportName,
                opt => opt.MapFrom(src => src.Coach.Sport.Name))
            .ForMember(dest => dest.CoachName,
                opt => opt.MapFrom(src => src.Coach.Employee.FirstName))
            .ForMember(dest => dest.BranchName,
                opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.StartTime,
                opt => opt.MapFrom(src => src.GroupSchedules
                                            .Select(gs => gs.StartTime)
                                            .FirstOrDefault())
            );
    }
}
