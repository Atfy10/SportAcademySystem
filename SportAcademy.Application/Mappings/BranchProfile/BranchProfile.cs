using SportAcademy.Application.Commands.BranchCommands.CreateBranch;
using SportAcademy.Application.Commands.BranchCommands.UpdateBranch;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.BranchProfile
{
    public class BranchProfile : AutoMapper.Profile
    {
        public BranchProfile()
        {
            CreateMap<Branch, BranchDropDownListDto>().ReverseMap();

            CreateMap<Branch, CreateBranchCommand>().ReverseMap();

            CreateMap<Branch, BranchDto>().ReverseMap();

            CreateMap<Branch, BranchCardDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CoX, opt => opt.MapFrom(src => src.CoX))
                .ForMember(dest => dest.CoY, opt => opt.MapFrom(src => src.CoY));

            CreateMap<Branch, UpdateBranchCommand>()
                .ReverseMap()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
