using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.ChatCommands.SendMessageToBot
{
    public class SendMessageToBotCommandHandler
        : IRequestHandler<SendMessageToBotCommand, Result<ChatMessageDto>>
    {
        private readonly IChatBotService _chatBotService;
        private readonly IChatMessageRepository _messageRepository;
        private readonly IChatConversationRepository _conversationRepository;
        private readonly string _operation = "Add";

        public SendMessageToBotCommandHandler(
            IChatBotService chatBotService,
            IChatMessageRepository messageRepository,
            IChatConversationRepository conversationRepository)
        {
            _chatBotService = chatBotService;
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
        }

        public async Task<Result<ChatMessageDto>> Handle(
            SendMessageToBotCommand request,
            CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository
                .GetByIdAsync(request.ConversationId, cancellationToken)
                ?? throw new ChatConversationNotFoundException(request.ConversationId.ToString());

            var userMessage = OpenAiMessage.Create(
                Guid.NewGuid(), conversation.Id, ChatRole.User, request.Message);

            await _messageRepository.AddAsync(userMessage, cancellationToken);

            var reply = await _chatBotService
                .GenerateBotReplyAsync(conversation.Id, cancellationToken);

            var botMessage = OpenAiMessage.Create(
                Guid.NewGuid(), conversation.Id, ChatRole.Assistant, reply);

            await _messageRepository.AddAsync(botMessage, cancellationToken);

            var dto = botMessage.ToDto();

            return Result<ChatMessageDto>.Success(dto, _operation);
        }
    }
}
