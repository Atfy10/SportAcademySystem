CREATE OR ALTER VIEW dbo.vw_EmployeeBasic AS
SELECT
    Id,
    FirstName,
    LastName,
    SSN,
    Email,
    BirthDate,
    Gender,
    City,
    SecondPhoneNumber
FROM Employees;