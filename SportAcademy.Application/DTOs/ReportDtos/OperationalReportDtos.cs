namespace SportAcademy.Application.DTOs.ReportDtos;

public record AttendanceReportRowDto(
    int Id,
    string Status,
    string CheckInTime,
    string CoachNote,
    int TraineeId,
    string TraineeName);

// One entry per session actually held (a distinct TraineeGroup + AttendanceDate combination) -
// GroupName/BranchName/CoachName/AttendanceDate are shared by every trainee marked for that
// session, so the report groups by them instead of repeating the same four columns on every
// trainee row.
public record AttendanceSessionGroupDto(
    int TraineeGroupId,
    string TraineeGroupName,
    string BranchName,
    string CoachName,
    DateTime AttendanceDate,
    List<AttendanceReportRowDto> Trainees);
