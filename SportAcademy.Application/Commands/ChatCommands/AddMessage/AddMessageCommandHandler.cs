using SportAcademy.Application.Contract;
using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Application.Contract;
using SportAcademy.Application.DTOs.ChatCommand;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions;
using SportAcademy.Shared.Result;

namespace SportAcademy.Application.Commands.ChatCommands.AddMessage
{
    public class AddMessageCommandHandler
    : IRequestHandler<AddMessageCommand, Result<ChatMessageDto>>
    {
        private readonly IChatConversationRepository _conversationRepository;
        private readonly IChatMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly string _operation = "AddMessage";

        public AddMessageCommandHandler(
            IChatConversationRepository conversationRepository,
            IChatMessageRepository messageRepository,
            IMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        public async Task<Result<ChatMessageDto>> Handle(
            AddMessageCommand request,
            CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository
                .GetByIdAsync(request.ConversationId, cancellationToken)
                ?? throw new ChatConversationNotFoundException(request.ConversationId.ToString());

            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatConversationId = conversation.Id,
                Role = request.Role,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _messageRepository.AddAsync(message, cancellationToken);

            var dto = _mapper.Map<ChatMessageDto>(message);

            return Result<ChatMessageDto>.Success(dto, _operation);
        }
    }
}
