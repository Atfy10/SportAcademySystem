using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;

namespace SportAcademy.Application.Queries.SubscriptionDiscountRequestQueries.GetSubscriptionDiscountRequestById;

public record GetSubscriptionDiscountRequestByIdQuery(int Id) : IRequest<Result<SubscriptionDiscountRequestDto>>;
