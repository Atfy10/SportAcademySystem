CREATE OR ALTER VIEW dbo.vw_ScheduleDaily AS
SELECT
    TraineeGroupId,
    Day,
    StartTime
FROM GroupSchedules
WHERE Day = DATENAME(WEEKDAY, GETDATE());