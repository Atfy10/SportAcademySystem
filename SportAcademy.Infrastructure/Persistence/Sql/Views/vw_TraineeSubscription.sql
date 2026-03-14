CREATE OR ALTER VIEW dbo.vw_TraineeSubscription AS
SELECT
    t.Id,
    t.FirstName,
    t.IsSubscribed,
    t.GuardianName,
    SD.StartDate,
    SD.EndDate,
    SD.PaymentNumber,
    ST.Name AS SubscriptionTypeName,
    S.Name AS SportName,
    B.Name AS BranchName
FROM Trainees t
JOIN SubscriptionDetails SD ON t.Id = SD.TraineeId
JOIN SubscriptionTypes ST ON SD.SubscriptionTypeId = ST.Id
JOIN SportSubscriptionTypes SCT ON ST.Id = SCT.SubscriptionTypeId
JOIN Sports S ON SCT.SportId = S.Id
JOIN SportBranches SB ON S.Id = SB.SportId
JOIN Branches B ON SB.BranchId = B.Id;