namespace SportAcademy.Application.DTOs.ImportDtos
{
    // Shapes shared by every CSV import (trainees, employees).

    // One data row as read from the file: its 1-based line number in the file (header = 1) and
    // its cells keyed by the header text exactly as written in the file.
    public record ImportRawRow(int RowNumber, IReadOnlyDictionary<string, string?> Cells);

    public record ImportCellError(string Column, string? Value, string Message);

    public static class ImportRowStatus
    {
        public const string Valid = "valid";
        public const string Invalid = "invalid";
        public const string Imported = "imported";
        public const string Failed = "failed";
    }

    public record ImportColumnInfo(
        string Key, string Label, bool Required, string Description, string Example, List<string>? AllowedValues);
}
