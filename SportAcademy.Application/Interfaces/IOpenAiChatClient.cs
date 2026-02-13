using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IOpenAiChatClient
    {
        Task<string> SendAsync(
            IReadOnlyList<OpenAiMessage> messages,
            CancellationToken cancellationToken);
    }
}
