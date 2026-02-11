using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Domain.Enums;


namespace SportAcademy.Application.DTOs.ChatDtos
{
    public record ChatMessageDto(
        Guid Id,
        ChatRole Role,
        string Content,
        DateTime CreatedAt
    );
}
