using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.DashboardDtos;
using SportAcademy.Application.Queries.AttendanceQueries.GetGlobalAttendanceRate;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoachesCount;
using SportAcademy.Application.Queries.EnrollmentQueries.GetEnrollmentsCountForSport;
using SportAcademy.Application.Queries.NotificationQueries.GetUserNotifications;
using SportAcademy.Application.Queries.SportQueries.GetAll;
using SportAcademy.Application.Queries.TraineeGroupQueries.GetAllOfSpecificDay;
using SportAcademy.Application.Queries.TraineeQueries.GetTraineesCountOfSpecificDay;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.DashboardQueries.GetDashboardSummary;

// Composes the same data the frontend used to assemble client-side from 15-20 separate
// requests (one per sport for enrollment counts, one per month for the attendance trend,
// plus the base widgets) into a single server-side round trip with one point-in-time view.
public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    private readonly IMediator _mediator;
    private const string Operation = "GetDashboardSummary";

    public GetDashboardSummaryQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        // Sequential, not Task.WhenAll: every one of these _mediator.Send calls ultimately
        // shares the same scoped ApplicationDbContext (same request scope), and DbContext is
        // not thread-safe for concurrent operations. Running them in parallel intermittently
        // threw "A second operation was started on this context instance before a previous
        // operation completed" depending on exact timing - the dashboard's random failures.
        var sportsResult = await _mediator.Send(new GetAllSportsQuery(), ct);
        var todayGroupsResult = await _mediator.Send(
            new GetAllSessionsOfSpecificDayQuery(today, PageRequest.Create(1, 4)), ct);
        var activeCoachesResult = await _mediator.Send(new GetActiveCoachesCountQuery(), ct);
        var overallAttendanceResult = await _mediator.Send(new GetGlobalAttendanceRateQuery(null, null), ct);
        var todayTraineesResult = await _mediator.Send(new GetTraineesCountOfSpecificDayQuery(today), ct);
        var notificationsResult = await _mediator.Send(
            new GetUserNotificationsQuery(PageRequest.Create(1, 10)), ct);

        var sports = sportsResult.Data ?? [];

        var enrollmentCounts = new List<SportEnrollmentCountDto>();
        foreach (var sport in sports)
        {
            var countResult = await _mediator.Send(
                new GetEnrollmentsCountForSportQuery(sport.Id, null, null), ct);
            enrollmentCounts.Add(new SportEnrollmentCountDto(sport.Id, sport.Name, countResult.Data));
        }

        var attendanceTrend = new List<MonthlyAttendanceDto>();
        for (var i = request.MonthTrendCount - 1; i >= 0; i--)
        {
            var target = today.AddMonths(-i);
            var rateResult = await _mediator.Send(
                new GetGlobalAttendanceRateQuery((Month)target.Month, target.Year), ct);
            attendanceTrend.Add(new MonthlyAttendanceDto(target.Month, target.Year, rateResult.IsSuccess ? rateResult.Data : 0));
        }

        var summary = new DashboardSummaryDto(
            sports,
            todayGroupsResult.Data!,
            activeCoachesResult.Data,
            overallAttendanceResult.Data,
            todayTraineesResult.Data,
            enrollmentCounts,
            attendanceTrend,
            notificationsResult.Data!
        );

        return Result<DashboardSummaryDto>.Success(summary, Operation);
    }
}
