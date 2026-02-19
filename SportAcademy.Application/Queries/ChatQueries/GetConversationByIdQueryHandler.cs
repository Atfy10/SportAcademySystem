using SportAcademy.Application.Interfaces;
using SportAcademy.Application.DTOs.ChatDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Domain.Exceptions;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.ChatQueries
{
    public class GetConversationByIdQueryHandler
    : IRequestHandler<GetConversationByIdQuery, Result<ChatConversationDto>>
    {
        private readonly IChatConversationRepository _repository;
        private readonly IMapper _mapper;
        private readonly string _operation = "Get";

        public GetConversationByIdQueryHandler(
            IChatConversationRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<ChatConversationDto>> Handle(
            GetConversationByIdQuery request,
            CancellationToken cancellationToken)
        {
            var conversation = await _repository
                .GetByIdAsync(request.Id, cancellationToken)
                ?? throw new ChatConversationNotFoundException(request.Id.ToString());

            var dto = _mapper.Map<ChatConversationDto>(conversation);

            return Result<ChatConversationDto>.Success(dto, _operation);
        }
    }
}
