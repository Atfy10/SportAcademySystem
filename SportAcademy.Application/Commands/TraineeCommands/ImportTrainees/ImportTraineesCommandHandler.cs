using MediatR;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Services.TraineeImport;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.Trainees.ImportTrainees
{
    public class ValidateTraineeImportCommandHandler
        : IRequestHandler<ValidateTraineeImportCommand, Result<TraineeImportReport>>
    {
        private readonly TraineeImportValidator _validator;

        public ValidateTraineeImportCommandHandler(TraineeImportValidator validator) => _validator = validator;

        public async Task<Result<TraineeImportReport>> Handle(ValidateTraineeImportCommand request, CancellationToken ct)
        {
            var result = await _validator.ValidateAsync(request.Rows, request.Headers, ct);
            // Always a successful envelope: the report IS the answer, problems included - the
            // screen renders it row by row rather than showing one generic failure.
            return Result<TraineeImportReport>.Success(result.Report, OperationType.Get.ToString());
        }
    }

    public class ImportTraineesCommandHandler
        : IRequestHandler<ImportTraineesCommand, Result<TraineeImportReport>>
    {
        private readonly TraineeImportValidator _validator;
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizer;
        private readonly string _operationType = OperationType.Add.ToString();

        public ImportTraineesCommandHandler(
            TraineeImportValidator validator,
            IMediator mediator,
            IUnitOfWork unitOfWork,
            ILocalizationService localizer)
        {
            _validator = validator;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<Result<TraineeImportReport>> Handle(ImportTraineesCommand request, CancellationToken ct)
        {
            // Re-validated here, not trusted from the dry run: data may have changed since (another
            // user registered the same phone, the plan's headroom was used up).
            var validation = await _validator.ValidateAsync(request.Rows, request.Headers, ct);
            var report = validation.Report;

            if (report.FileErrors.Count > 0 || (!request.ImportValidRowsOnly && report.InvalidRows > 0))
                return Result<TraineeImportReport>.Success(report, _operationType);

            var rows = report.Rows.ToDictionary(r => r.RowNumber);
            var imported = 0;
            var failed = 0;

            foreach (var (rowNumber, command) in validation.ValidRows)
            {
                ct.ThrowIfCancellationRequested();

                // Each row is its own transaction, so a row that still fails (a race with another
                // user, say) rolls back completely - no orphan family left behind - and its
                // half-built entities are dropped from the change tracker instead of being
                // retried by every later row's save.
                await _unitOfWork.BeginTransactionAsync(ct);
                try
                {
                    var result = await _mediator.Send(command, ct);
                    if (result.IsSuccess)
                    {
                        await _unitOfWork.CommitTransactionAsync(ct);
                        imported++;
                        rows[rowNumber] = rows[rowNumber] with
                        {
                            Status = TraineeImportRowStatus.Imported,
                            TraineeId = result.Data?.TraineeId,
                            TraineeCode = result.Data?.Code,
                        };
                        continue;
                    }

                    await _unitOfWork.RollbackTransactionAsync(ct);
                    _unitOfWork.ClearChangeTracker();
                    failed++;
                    // Field-level messages for a validation failure; otherwise the (already
                    // localized) message - e.g. the plan-limit or a duplicate-phone explanation.
                    var detail = result.Code == "errors.validation.failed" && result.Errors is { Count: > 0 }
                        ? string.Join(" ", result.Errors.SelectMany(e => e.Value).Distinct())
                        : result.Message;
                    rows[rowNumber] = rows[rowNumber] with
                    {
                        Status = TraineeImportRowStatus.Failed,
                        Errors = [new TraineeImportCellError(string.Empty, null, _localizer["import.row.failed", detail ?? string.Empty])],
                    };
                }
                catch (OperationCanceledException)
                {
                    await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                    throw;
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                    _unitOfWork.ClearChangeTracker();
                    failed++;
                    rows[rowNumber] = rows[rowNumber] with
                    {
                        Status = TraineeImportRowStatus.Failed,
                        Errors = [new TraineeImportCellError(string.Empty, null, _localizer["import.row.failed", _localizer["errors.generic"]])],
                    };
                }
            }

            var final = report with
            {
                ImportedRows = imported,
                FailedRows = failed,
                Committed = true,
                Rows = report.Rows.Select(r => rows[r.RowNumber]).ToList(),
            };

            return Result<TraineeImportReport>.Success(
                final, _operationType, _localizer["import.done", imported, report.TotalRows]);
        }
    }
}
