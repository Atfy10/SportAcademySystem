using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class ChatMappings
    {
        public static ChatMessageDto ToDto(this OpenAiMessage message)
            => new(message.Id, message.Role, message.Content, message.CreatedAt);

        public static ChatConversationDto ToDto(this ChatConversation conversation)
            => new(conversation.Id, conversation.Title, conversation.CreatedAt,
                conversation.Messages.Select(m => m.ToDto()).ToList().AsReadOnly());
    }
}
