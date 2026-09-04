using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.DiscountCodeDtos;

namespace SportAcademy.Application.Queries.DiscountCodeQueries.ValidateDiscountCode
{
    public record ValidateDiscountCodeQuery(string Code) : IRequest<Result<DiscountCodeValidationDto>>;
}
