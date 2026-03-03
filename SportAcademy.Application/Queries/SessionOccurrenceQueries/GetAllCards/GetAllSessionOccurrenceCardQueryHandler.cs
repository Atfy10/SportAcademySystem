using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.SessionOccurrenceQueries.GetAllCards
{
    public class GetAllSessionOccurrenceCardQueryHandler: IRequestHandler<GetAllSessionOccurrenceCardQuery, Result<PagedData<SessionOccurrenceCardDto>>>
    {
        private readonly ISessionOccurrenceRepository _sessionOccurrenceRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.GetAll.ToString();
        public GetAllSessionOccurrenceCardQueryHandler(ISessionOccurrenceRepository sessionOccurrenceRepository, IMapper mapper)
        {
            _sessionOccurrenceRepository = sessionOccurrenceRepository;
            _mapper = mapper;
        }
        public async Task<Result<PagedData<SessionOccurrenceCardDto>>> Handle(GetAllSessionOccurrenceCardQuery request, CancellationToken cancellationToken)
        {
            var pagedData = await _sessionOccurrenceRepository
             .GetAllAsync(request.Page, cancellationToken);

            return Result<PagedData<SessionOccurrenceCardDto>>
                .Success(pagedData, _operationType);
        }
    }
}
