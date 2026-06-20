using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Commands.ChatCommands.CreateConversation
{
    public class CreateConversationCommandHandler
    : IRequestHandler<CreateConversationCommand, Result<ChatConversationDto>>
    {
        private readonly IChatConversationRepository _conversationRepository;
        private readonly string _operation = "Add";

        public CreateConversationCommandHandler(
            IChatConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<Result<ChatConversationDto>> Handle(
            CreateConversationCommand request,
            CancellationToken cancellationToken)
        {
            var conversation = ChatConversation.Create(Guid.NewGuid(), request.Title);

            await _conversationRepository.AddAsync(conversation, cancellationToken);

            var dto = conversation.ToDto();

            return Result<ChatConversationDto>.Success(dto, _operation);
        }
    }
}
