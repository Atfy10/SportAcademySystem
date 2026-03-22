using AutoMapper;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.NotificationProfile
{
    public class NotificationMappingProfile : AutoMapper.Profile
    {
        public NotificationMappingProfile()
        {
            CreateMap<NotificationRecipient, NotificationRecipientDto>()
                .ConstructUsing(src => new NotificationRecipientDto
                {
                    Id = src.NotificationId,
                    Title = src.Notification.GroupName ?? "Notification",
                    Message = src.Notification.Message,
                    Type = src.Notification.GroupName != null ? "group" : "system",
                    IsRead = src.IsRead,
                    CreatedAt = src.Notification.CreatedAt
                });
        }
    }
}
