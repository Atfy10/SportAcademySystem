using SportAcademy.Application.Interfaces;
using SportAcademy.Application.DTOs.ChatDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions;
using SportAcademy.Domain.Enums;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.ChatCommands.AddMessage
{
    public class AddMessageCommandHandler
    : IRequestHandler<AddMessageCommand, Result<ChatMessageDto>>
    {
        private readonly IChatConversationRepository _conversationRepository;
        private readonly IChatMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly string _operation = OperationType.Add.ToString();

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

            var message = new OpenAiMessage
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
