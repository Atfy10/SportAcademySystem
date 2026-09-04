using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.DiscountCodeDtos;

namespace SportAcademy.Application.Queries.DiscountCodeQueries.GetAll
{
    public record GetAllDiscountCodesQuery() : IRequest<Result<List<DiscountCodeDto>>>;
}
