using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.Trainees.ImportTrainees
{
    // Headers: the file's header row as written. Rows: every data row, keyed by those headers.
    // ImportValidRowsOnly: true = save the rows that pass validation and skip the rest (the user
    // saw which ones in the dry run); false = save nothing unless every row is valid.
    public record ImportTraineesCommand(
        IReadOnlyList<string> Headers,
        IReadOnlyList<TraineeImportRawRow> Rows,
        bool ImportValidRowsOnly = true)
        : IRequest<Result<TraineeImportReport>>, IRequiresFeature
    {
        public string FeatureKey => "trainee-management";
    }

    // The dry run: the same full validation, nothing written.
    public record ValidateTraineeImportCommand(
        IReadOnlyList<string> Headers,
        IReadOnlyList<TraineeImportRawRow> Rows)
        : IRequest<Result<TraineeImportReport>>, IRequiresFeature
    {
        public string FeatureKey => "trainee-management";
    }
}
