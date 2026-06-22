using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.Trainees.ImportTrainees
{
    public class ImportTraineesCommandHandler
        : IRequestHandler<ImportTraineesCommand, Result<ImportTraineesResult>>
    {
        private readonly IMediator _mediator;
        private readonly string _operationType = OperationType.Add.ToString();

        public ImportTraineesCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<ImportTraineesResult>> Handle(
            ImportTraineesCommand request,
            CancellationToken cancellationToken)
        {
            var result = new ImportTraineesResult
            {
                TotalRows = request.Trainees.Count
            };

            for (int i = 0; i < request.Trainees.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var createResult = await _mediator.Send(
                        request.Trainees[i], cancellationToken);

                    if (createResult.IsSuccess)
                        result.SuccessCount++;
                    else
                    {
                        result.FailureCount++;
                        result.Errors.Add(new ImportRowError
                        {
                            RowNumber = i + 2,
                            Message = createResult.Message
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add(new ImportRowError
                    {
                        RowNumber = i + 2,
                        Message = ex.InnerException?.Message ?? ex.Message
                    });
                }
            }

            return Result<ImportTraineesResult>.Success(
                result,
                _operationType,
                $"{result.SuccessCount} of {result.TotalRows} trainees imported successfully.");
        }
    }
}
