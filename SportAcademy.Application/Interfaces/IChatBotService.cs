using SportAcademy.Application.DTOs.ChatDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IChatBotService
    {
        Task<string> GenerateBotReplyAsync(
           Guid conversationId,
           CancellationToken cancellationToken);
    }
}
