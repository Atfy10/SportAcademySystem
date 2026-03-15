using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.SportQueries.GetAll;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SportQueries.GetAllSportsPaginated
{
    public class GetAllSportsPaginatedQueryHandler : IRequestHandler<GetAllSportsPaginatedQuery, Result<PagedData<SportDto>>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllSportsPaginatedQueryHandler(
            ISportRepository sportRepository)
        {
            _sportRepository = sportRepository;
        }

        public async Task<Result<PagedData<SportDto>>> Handle(GetAllSportsPaginatedQuery request, CancellationToken cancellationToken)
        {
            var sports = await _sportRepository.GetAllPaginatedAsync<SportDto>(
                request.Page, cancellationToken);

            return Result<PagedData<SportDto>>.Success(sports, _operationType);
        }

    }
}
