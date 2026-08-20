using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.DashboardDtos;

namespace SportAcademy.Application.Queries.DashboardQueries.GetDashboardSummary;

// MonthTrendCount controls how many trailing calendar months (including the current one)
// are included in AttendanceTrend, matching what the dashboard chart displays.
public record GetDashboardSummaryQuery(int MonthTrendCount = 5) : IRequest<Result<DashboardSummaryDto>>;
