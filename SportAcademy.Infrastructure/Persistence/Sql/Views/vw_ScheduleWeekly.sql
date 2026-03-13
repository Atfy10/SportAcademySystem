CREATE OR ALTER VIEW dbo.vw_ScheduleWeekly AS
SELECT
    TraineeGroupId,
    Day,
    StartTime
FROM GroupSchedules;