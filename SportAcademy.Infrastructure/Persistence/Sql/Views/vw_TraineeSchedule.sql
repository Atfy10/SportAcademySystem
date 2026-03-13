CREATE OR ALTER VIEW dbo.vw_TraineeSchedule AS
SELECT
    TraineeGroupId,
    Day,
    StartTime
FROM GroupSchedules;