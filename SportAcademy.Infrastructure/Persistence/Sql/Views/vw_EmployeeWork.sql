CREATE OR ALTER VIEW dbo.vw_EmployeeWork AS
SELECT
    E.Id,
    E.Salary,
    E.HireDate,
    E.Position,
    B.Name AS BranchName,
    A.UserName
FROM Employees E
JOIN Branches B ON B.Id = E.BranchId
JOIN AspNetUsers A ON E.AppUserId = A.Id;