using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Services
{
    public class ChatBotService : IChatBotService
    {
        private readonly IChatMessageRepository _messageRepository;
        private readonly IOpenAiChatClient _openAiClient;

        public ChatBotService(
            IChatMessageRepository messageRepository,
            IOpenAiChatClient openAiClient)
        {
            _messageRepository = messageRepository;
            _openAiClient = openAiClient;
        }

        public async Task<string> GenerateBotReplyAsync(
            Guid conversationId,
            CancellationToken cancellationToken)
        {
            // read history
            var history = await _messageRepository
                .GetByConversationIdAsync(conversationId, cancellationToken);

            var aiMessages = history
                .Select(m => new OpenAiMessage
                {
                    Role = m.Role,
                    Content = m.Content
                })
                .ToList();

            // call AI
            var response = await _openAiClient.SendAsync(aiMessages, cancellationToken);

            return response;
        }
    }
}

