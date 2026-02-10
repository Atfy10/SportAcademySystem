using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.ChatDtos
{
    public record ChatConversationDto(
    Guid Id,
    string? Title,
    DateTime CreatedAt,
    IReadOnlyCollection<ChatMessageDto> Messages
);
}
