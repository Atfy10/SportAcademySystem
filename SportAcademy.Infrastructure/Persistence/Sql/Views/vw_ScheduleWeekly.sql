CREATE OR ALTER VIEW dbo.vw_ScheduleWeekly AS
SELECT
    gs.TenantId,
    gs.TraineeGroupId,
    gs.Day,
    gs.StartTime
FROM GroupSchedules gs;