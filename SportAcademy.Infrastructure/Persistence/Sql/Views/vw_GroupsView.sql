CREATE OR ALTER VIEW dbo.vw_GroupsView AS
SELECT
    tg.Id AS TraineeGroupId,
    tg.SkillLevel,
    tg.MaximumCapacity,
    tg.DurationInMinutes,
    tg.Gender,
    b.Name AS BranchName,
    e.FirstName AS CoachName
FROM TraineeGroups tg
JOIN Branches b ON tg.BranchId = b.Id
JOIN Employees e ON b.Id = e.BranchId
JOIN Coaches c ON e.Id = c.EmployeeId;