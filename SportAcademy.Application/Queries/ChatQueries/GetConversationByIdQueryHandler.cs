using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Queries.ChatQueries
{
    public class GetConversationByIdQueryHandler
    : IRequestHandler<GetConversationByIdQuery, Result<ChatConversationDto>>
    {
        private readonly IChatConversationRepository _repository;
        private readonly string _operation = "Get";

        public GetConversationByIdQueryHandler(
            IChatConversationRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ChatConversationDto>> Handle(
            GetConversationByIdQuery request,
            CancellationToken cancellationToken)
        {
            var conversation = await _repository
                .GetByIdAsync(request.Id, cancellationToken)
                ?? throw new ChatConversationNotFoundException(request.Id.ToString());

            var dto = conversation.ToDto();

            return Result<ChatConversationDto>.Success(dto, _operation);
        }
    }
}
