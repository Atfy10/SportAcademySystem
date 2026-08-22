CREATE OR ALTER PROCEDURE usp_GenerateDocumentNumber
    @TenantId UNIQUEIDENTIFIER,
    @DocumentType NVARCHAR(10),
    @Year INT,
    @DocumentNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Counter TABLE (LastNumber INT)

    ------------------------------------------------
    -- Atomic per-tenant, per-document-type, per-year counter increment. MERGE handles the
    -- first-number-of-the-year case (no row yet) and every subsequent call (row exists) in one
    -- statement, so there is no separate "does a row exist" check that could race with another
    -- session between reading and writing it.
    ------------------------------------------------

    MERGE DocumentNumberCounters AS target
    USING (SELECT @TenantId AS TenantId, @DocumentType AS DocumentType, @Year AS [Year]) AS src
        ON target.TenantId = src.TenantId
           AND target.DocumentType = src.DocumentType
           AND target.[Year] = src.[Year]
    WHEN MATCHED THEN
        UPDATE SET LastNumber = target.LastNumber + 1
    WHEN NOT MATCHED THEN
        INSERT (TenantId, DocumentType, [Year], LastNumber)
        VALUES (src.TenantId, src.DocumentType, src.[Year], 1)
    OUTPUT INSERTED.LastNumber INTO @Counter(LastNumber);

    DECLARE @Number INT = (SELECT LastNumber FROM @Counter)

    IF @Number > 99999
    BEGIN
        THROW 50005, 'Document number limit exceeded for this year', 1
    END

    SET @DocumentNumber =
        CONCAT(@DocumentType, '-', @Year, '-', RIGHT('00000' + CAST(@Number AS VARCHAR(5)), 5))
END
