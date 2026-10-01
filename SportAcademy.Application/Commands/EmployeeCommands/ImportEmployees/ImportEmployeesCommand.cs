using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.DTOs.ImportDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EmployeeCommands.ImportEmployees
{
    // Headers: the file's header row as written. Rows: every data row, keyed by those headers.
    // ImportValidRowsOnly: true = save the rows that pass validation and skip the rest (the user
    // saw which ones in the dry run); false = save nothing unless every row is valid.
    public record ImportEmployeesCommand(
        IReadOnlyList<string> Headers,
        IReadOnlyList<ImportRawRow> Rows,
        bool ImportValidRowsOnly = true)
        : IRequest<Result<EmployeeImportReport>>, IRequiresFeature
    {
        public string FeatureKey => "employee-management";
    }

    // The dry run: the same full validation, nothing written.
    public record ValidateEmployeeImportCommand(
        IReadOnlyList<string> Headers,
        IReadOnlyList<ImportRawRow> Rows)
        : IRequest<Result<EmployeeImportReport>>, IRequiresFeature
    {
        public string FeatureKey => "employee-management";
    }
}
