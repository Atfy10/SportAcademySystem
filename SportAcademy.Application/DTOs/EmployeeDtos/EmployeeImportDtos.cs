using SportAcademy.Application.DTOs.ImportDtos;

namespace SportAcademy.Application.DTOs.EmployeeDtos
{
    public record EmployeeImportRowReport(
        int RowNumber,
        string? EmployeeName,
        string Status,
        List<ImportCellError> Errors,
        int? EmployeeId = null);

    // Validation (dry run) and commit share this shape. FileErrors are problems with the file as
    // a whole (missing required columns, no rows) - nothing is imported while any exist.
    public record EmployeeImportReport(
        int TotalRows,
        int ValidRows,
        int InvalidRows,
        int ImportedRows,
        int FailedRows,
        bool Committed,
        List<string> FileErrors,
        List<string> UnknownColumns,
        List<EmployeeImportRowReport> Rows);

    // What the import screen shows next to the upload box, and what the template is built from.
    public record EmployeeImportTemplateDto(
        List<ImportColumnInfo> Columns,
        List<Dictionary<string, string>> SampleRows,
        List<string> Branches,
        List<string> Positions,
        List<string> Genders,
        List<string> Nationalities);
}
