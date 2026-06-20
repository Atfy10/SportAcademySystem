using AutoMapper;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Mappings
{
    public class BranchMappingProfile : AutoMapper.Profile
    {
        public BranchMappingProfile()
        {
            CreateMap<Branch, BranchDto>()
                .ConstructUsing(src => new BranchDto
                {
                    Id = src.Id,
                    Name = src.Name,
                    City = src.City,
                    Country = src.Country,
                    PhoneNumber = src.PhoneNumber,
                    Email = src.Email,
                    CoX = src.CoX,
                    CoY = src.CoY,
                    IsActive = src.IsActive
                });

            CreateMap<Branch, BranchDropDownListDto>()
                .ConstructUsing(src => new BranchDropDownListDto(
                    src.Id,
                    src.Name
                ));

            CreateMap<Branch, BranchCardDto>()
                .ConstructUsing(src => new BranchCardDto
                {
                    Id = src.Id,
                    Name = src.Name,
                    City = src.City,
                    Country = src.Country,
                    PhoneNumber = src.PhoneNumber,
                    Email = src.Email,
                    CoX = src.CoX,
                    CoY = src.CoY
                });
        }
    }
}
