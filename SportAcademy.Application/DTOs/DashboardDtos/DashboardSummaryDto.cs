using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.DTOs.TraineeGroupDtos;

namespace SportAcademy.Application.DTOs.DashboardDtos;

public record DashboardSummaryDto(
    IReadOnlyList<SportDto> Sports,
    PagedData<ListTraineeGroupDto> TodayGroups,
    int ActiveCoachesCount,
    int OverallAttendanceRate,
    int TodayTraineesCount,
    IReadOnlyList<SportEnrollmentCountDto> EnrollmentCountsBySport,
    IReadOnlyList<MonthlyAttendanceDto> AttendanceTrend,
    PagedData<NotificationRecipientDto> RecentNotifications
);

public record SportEnrollmentCountDto(int SportId, string SportName, int Count);

public record MonthlyAttendanceDto(int Month, int Year, int Rate);
