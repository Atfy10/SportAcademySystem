CREATE OR ALTER VIEW dbo.vw_TraineeAttendance AS
SELECT
    so.StartDateTime,
    so.Status,
    a.AttendanceDate,
    a.AttendanceStatus,
    a.CheckInTime,
    a.CoachNote
FROM Attendances a
JOIN SessionOccurrences so ON a.SessionOccurrenceId = so.Id;