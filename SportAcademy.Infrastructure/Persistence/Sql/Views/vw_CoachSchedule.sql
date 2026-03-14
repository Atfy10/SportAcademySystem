CREATE OR ALTER VIEW dbo.vw_CoachSchedule AS
SELECT
    tg.Id AS TraineeGroupId,
    tg.SkillLevel,
    tg.MaximumCapacity,
    tg.DurationInMinutes,
    tg.Gender,
    b.Name AS BranchName,
    em.FirstName AS CoachName
FROM TraineeGroups tg
JOIN Branches b ON tg.BranchId = b.Id
JOIN Coaches c ON tg.CoachId = c.EmployeeId
JOIN Employees em ON c.EmployeeId = em.Id;