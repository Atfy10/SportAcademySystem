using AutoMapper;
using SportAcademy.Application.Commands.BranchCommands.CreateBranch;
using SportAcademy.Application.Commands.BranchCommands.UpdateBranch;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Application.Mappings.BranchProfile
{
    public class BranchProfile : AutoMapper.Profile
    {
        public BranchProfile()
        {
            CreateMap<Branch, CreateBranchCommand>()
                .ForMember(dest => dest.CoX,
                       opt => opt.MapFrom(src => src.Coordinate.CoX))
                .ForMember(dest => dest.CoY,
                        opt => opt.MapFrom(src => src.Coordinate.CoY))
                .ReverseMap()
                .ForMember(dest => dest.Coordinate,
                       opt => opt.MapFrom(src => Coordinate.Create(src.CoX, src.CoY)));

            CreateMap<Branch, BranchDto>()
                .ForMember(dest => dest.CoX,
                       opt => opt.MapFrom(src => src.Coordinate.CoX))
                .ForMember(dest => dest.CoY,
                        opt => opt.MapFrom(src => src.Coordinate.CoY))
                .ReverseMap()
                .ForMember(dest => dest.Coordinate,
                       opt => opt.MapFrom(src => Coordinate.Create(src.CoX, src.CoY)));

            CreateMap<Branch, UpdateBranchCommand>()
                .ForMember(dest => dest.CoX,
                       opt => opt.MapFrom(src => src.Coordinate.CoX))
                .ForMember(dest => dest.CoY,
                        opt => opt.MapFrom(src => src.Coordinate.CoY))
                .ReverseMap()
                .ForMember(dest => dest.Coordinate,
                       opt => opt.MapFrom(src => Coordinate.Create(src.CoX, src.CoY)))
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }

    }
}
