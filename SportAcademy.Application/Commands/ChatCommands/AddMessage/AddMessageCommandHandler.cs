using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.ChatCommands.AddMessage
{
    public class AddMessageCommandHandler
    : IRequestHandler<AddMessageCommand, Result<ChatMessageDto>>
    {
        private readonly IChatConversationRepository _conversationRepository;
        private readonly IChatMessageRepository _messageRepository;
        private readonly string _operation = "Add";

        public AddMessageCommandHandler(
            IChatConversationRepository conversationRepository,
            IChatMessageRepository messageRepository)
        {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
        }

        public async Task<Result<ChatMessageDto>> Handle(
            AddMessageCommand request,
            CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository
                .GetByIdAsync(request.ConversationId, cancellationToken)
                ?? throw new ChatConversationNotFoundException(request.ConversationId.ToString());

            var message = OpenAiMessage.Create(
                Guid.NewGuid(), conversation.Id, request.Role, request.Content);

            await _messageRepository.AddAsync(message, cancellationToken);

            var dto = message.ToDto();

            return Result<ChatMessageDto>.Success(dto, _operation);
        }
    }
}
