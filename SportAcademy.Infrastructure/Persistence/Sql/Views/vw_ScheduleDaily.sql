CREATE OR ALTER VIEW dbo.vw_ScheduleDaily AS
SELECT
    gs.TenantId,
    gs.TraineeGroupId,
    gs.Day,
    gs.StartTime
FROM GroupSchedules gs
WHERE gs.Day = DATENAME(WEEKDAY, GETDATE());