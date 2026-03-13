CREATE OR ALTER VIEW dbo.vw_AdminBasic AS
SELECT
    A.UserName,
    E.FirstName,
    E.LastName,
    E.SSN,
    E.Email,
    E.BirthDate,
    E.Gender,
    E.City,
    E.SecondPhoneNumber,
    P.ProfileImageUrl,
    P.Bio,
    P.CreatedAt
FROM Employees E
JOIN AspNetUsers A ON E.AppUserId = A.Id
JOIN Profiles P ON A.Id = P.AppUserId;