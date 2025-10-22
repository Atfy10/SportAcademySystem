using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.EnrollmentProfile
{
    public class EnrollmentMappingProfile : AutoMapper.Profile
    {
        public EnrollmentMappingProfile()
        {
            CreateMap<Enrollment, EnrollmentDto>()
                .ReverseMap();

            CreateMap<CreateEnrollmentCommand, Enrollment>();

            CreateMap<UpdateEnrollmentCommand, Enrollment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
