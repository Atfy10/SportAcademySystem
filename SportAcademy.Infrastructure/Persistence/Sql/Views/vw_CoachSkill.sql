CREATE OR ALTER VIEW dbo.vw_CoachSkill AS
SELECT
    c.EmployeeId AS Id,
    c.SkillLevel,
    s.Name AS Sport
FROM Coaches c
JOIN Sports s ON c.SportId = s.Id;