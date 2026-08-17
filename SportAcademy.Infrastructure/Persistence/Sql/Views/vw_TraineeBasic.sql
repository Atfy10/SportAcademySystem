CREATE OR ALTER VIEW dbo.vw_TraineeBasic AS
SELECT
    TenantId,
    Id,
    FirstName,
    LastName,
    SSN,
    Email,
    BirthDate,
    Gender,
    City,
    SecondPhoneNumber
FROM Trainees;