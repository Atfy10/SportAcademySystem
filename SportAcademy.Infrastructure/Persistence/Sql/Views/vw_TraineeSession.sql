CREATE OR ALTER VIEW dbo.vw_TraineeSession AS
SELECT
    t.Id,
    e.EnrollmentDate,
    e.ExpiryDate,
    e.SessionAllowed,
    e.SessionRemaining,
    tg.Id AS TraineeGroupId,
    tg.SkillLevel,
    tg.MaximumCapacity,
    tg.DurationInMinutes,
    tg.Gender,
    b.Name AS Branch,
    emp.FirstName + ' ' + emp.LastName AS Coach_Name
FROM Trainees t
JOIN Enrollments e ON t.Id = e.TraineeId
JOIN TraineeGroups tg ON tg.Id = e.TraineeGroupId
JOIN Coaches c ON c.EmployeeId = tg.CoachId
JOIN Employees emp ON emp.Id = c.EmployeeId
JOIN Branches b ON b.Id = emp.BranchId;