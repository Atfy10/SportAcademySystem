using SportAcademy.Application.DTOs.ImportDtos;

namespace SportAcademy.Application.DTOs.TraineeDtos
{
    // Raw rows, cell errors, row statuses and column descriptions are the shared shapes in
    // DTOs.ImportDtos; these are the trainee-specific report and template.
    public record TraineeImportRowReport(
        int RowNumber,
        string? TraineeName,
        string Status,
        List<ImportCellError> Errors,
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

    // What the import screen shows next to the upload box, and what the template is built from.
    public record TraineeImportTemplateDto(
        List<ImportColumnInfo> Columns,
        List<Dictionary<string, string>> SampleRows,
        List<string> Branches,
        List<string> NationalityCategories,
        List<string> Sports,
        List<string> Genders,
        List<string> Nationalities,
        int? RemainingTraineeSlots);
}
