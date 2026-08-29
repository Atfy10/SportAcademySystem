using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Queries.SportQueries.GetById
{
    public class GetSportByIdQueryHandler : IRequestHandler<GetSportByIdQuery, Result<SportDto>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetSportByIdQueryHandler(ISportRepository sportRepository)
        {
            _sportRepository = sportRepository;
        }

        public async Task<Result<SportDto>> Handle(GetSportByIdQuery request, CancellationToken cancellationToken)
        {
            var sportDto = await _sportRepository.GetTranslatedByIdAsync(request.Id, cancellationToken)
                ?? throw new SportNotFoundException($"{request.Id}");

            return Result<SportDto>.Success(sportDto, _operationType);
        }
    }
}
