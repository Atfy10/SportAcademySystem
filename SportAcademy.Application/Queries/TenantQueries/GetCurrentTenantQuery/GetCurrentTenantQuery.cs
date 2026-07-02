using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;

namespace SportAcademy.Application.Queries.TenantQueries.GetCurrentTenantQuery;

public record GetCurrentTenantQuery : IRequest<Result<CurrentTenantResponse>>;
