using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetAllForDropdown;

public record GetAllSubscriptionDetailsForDropdownQuery : IRequest<Result<List<SubscriptionDetailsDropdownDto>>>;
