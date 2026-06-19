using AutoMapper;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Mappings.NotificationProfile
{
    public class NotificationMappingProfile : AutoMapper.Profile
    {
        public NotificationMappingProfile()
        {
            CreateMap<NotificationRecipient, NotificationRecipientDto>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.NotificationId))
                .ForMember(dest => dest.Title,
                    opt => opt.MapFrom(src => src.Notification.Title ?? "Notification"))
                .ForMember(dest => dest.Message,
                    opt => opt.MapFrom(src => src.Notification.Message))
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Notification.Type ?? NotificationType.System))
                .ForMember(dest => dest.ActionUrl,
                    opt => opt.MapFrom(src => src.Notification.ActionUrl))
                .ForMember(dest => dest.IsRead,
                    opt => opt.MapFrom(src => src.IsRead))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.Notification.CreatedAt));
        }
    }
}
