CREATE PROCEDURE usp_GenerateTraineeCode
    @FamilyId INT,
    @BranchId INT,
    @NationalityCategoryId INT,
    @AgeCode CHAR(1),
    @TraineeCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FamilyCode INT
    DECLARE @BranchCode INT
    DECLARE @NationalityCode NVARCHAR(3)

    DECLARE @MemberNumber INT

    ------------------------------------------------
    -- Determine age category
    ------------------------------------------------

    IF @AgeCode NOT IN ('K','Y','A')
    BEGIN
        THROW 50001, 'Invalid Age Category Code', 1;
    END
    ------------------------------------------------
    -- Atomic counter increment + get family code
    ------------------------------------------------

    DECLARE @Family TABLE
    (
        MemberNumber INT,
        FamilyCode INT
    )

    UPDATE Families
    SET LastMemberNumber = LastMemberNumber + 1
    OUTPUT INSERTED.LastMemberNumber, INSERTED.FamilyCode
    INTO @Family(MemberNumber, FamilyCode)
    WHERE Id = @FamilyId
    IF @@ROWCOUNT = 0
    BEGIN
        THROW 50001, 'Family not found', 1
    END

    SELECT
        @MemberNumber = MemberNumber,
        @FamilyCode = FamilyCode
    FROM @Family
    IF @MemberNumber > 9999
    BEGIN
        THROW 50004, 'Family member limit exceeded', 1
    END
    ------------------------------------------------
    -- Get branch + nationality in one query
    ------------------------------------------------

    SELECT
        @BranchCode = Id
    FROM Branches
    WHERE Id = @BranchId

    IF @BranchCode IS NULL
        THROW 50002, 'Branch not found', 1

    SELECT
        @NationalityCode = Code
    FROM NationalityCategories
    WHERE Id = @NationalityCategoryId

    IF @NationalityCode IS NULL
        THROW 50003, 'Nationality category not found', 1
    ------------------------------------------------
    -- Build trainee code
    ------------------------------------------------

    SET @TraineeCode =
        CONCAT(
            @AgeCode, '-',
            @FamilyCode, '-',
            @BranchCode, '-',
            @NationalityCode, '-',
            RIGHT('0000' + CAST(@MemberNumber AS VARCHAR(4)), 4)
        )

    SELECT @TraineeCode AS TraineeCode
END