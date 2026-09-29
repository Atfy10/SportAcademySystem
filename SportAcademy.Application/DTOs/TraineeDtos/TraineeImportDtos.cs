namespace SportAcademy.Application.DTOs.TraineeDtos
{
    // One data row as read from the file: its 1-based line number in the file (header = 1) and
    // its cells keyed by the header text exactly as written in the file.
    public record TraineeImportRawRow(int RowNumber, IReadOnlyDictionary<string, string?> Cells);

    public record TraineeImportCellError(string Column, string? Value, string Message);

    public static class TraineeImportRowStatus
    {
        public const string Valid = "valid";
        public const string Invalid = "invalid";
        public const string Imported = "imported";
        public const string Failed = "failed";
    }

    public record TraineeImportRowReport(
        int RowNumber,
        string? TraineeName,
        string Status,
        List<TraineeImportCellError> Errors,
        int? TraineeId = null,
        string? TraineeCode = null);

    // Validation (dry run) and commit share this shape. FileErrors are problems with the file
    // as a whole (missing required columns, no rows) - nothing is imported while any exist.
    public record TraineeImportReport(
        int TotalRows,
        int ValidRows,
        int InvalidRows,
        int ImportedRows,
        int FailedRows,
        bool Committed,
        List<string> FileErrors,
        List<string> UnknownColumns,
        List<TraineeImportRowReport> Rows);

    public record TraineeImportColumnInfo(
        string Key, string Label, bool Required, string Description, string Example, List<string>? AllowedValues);

    // What the import screen shows next to the upload box, and what the template is built from.
    public record TraineeImportTemplateDto(
        List<TraineeImportColumnInfo> Columns,
        List<Dictionary<string, string>> SampleRows,
        List<string> Branches,
        List<string> NationalityCategories,
        List<string> Sports,
        List<string> Genders,
        List<string> Nationalities,
        int? RemainingTraineeSlots);
}
