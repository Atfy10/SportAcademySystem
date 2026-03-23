using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NationalityDtos;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.NationalityQueries.GetAll;

public class GetAllQueryHandler : IRequestHandler<GetAllQuery, Result<IReadOnlyList<NationalityDto>>>
{
    public async Task<Result<IReadOnlyList<NationalityDto>>> Handle(GetAllQuery request, CancellationToken cancellationToken)
    {
        var nationalities = Enum.GetValues<Nationality>()
            .Select(n => new NationalityDto
            {
                Id = (int)n,
                Name = n.ToString()
            })
            .ToList();

        return Result<IReadOnlyList<NationalityDto>>.Success(nationalities, nameof(GetAllQuery));
    }
}
