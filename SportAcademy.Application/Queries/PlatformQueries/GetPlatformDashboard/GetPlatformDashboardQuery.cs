using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetPlatformDashboard;

public record GetPlatformDashboardQuery : IRequest<Result<PlatformDashboardResponse>>;
