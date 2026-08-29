namespace SportAcademy.Application.Common.Result
{
    public abstract class ResultBase
    {
        public bool IsSuccess { get; protected set; }
        public string OperationType { get; protected set; } = default!;
        public string Message { get; protected set; } = default!;
        public int StatusCode { get; set; }
        public Dictionary<string, string[]>? Errors { get; protected set; }

        /// <summary>
        /// Stable, machine-readable error identifier, e.g. <c>errors.trainee.notFound</c>.
        /// </summary>
        /// <remarks>
        /// Doubles as the localization key and as the value the frontend branches on, so it can
        /// stop matching English prose. Null on success.
        /// </remarks>
        public string? Code { get; set; }

        /// <summary>
        /// Correlation id echoed from the request, so a user-facing "reference" ties a support
        /// report to the full diagnostic context in the logs.
        /// </summary>
        public string? TraceId { get; set; }

        protected ResultBase(bool isSuccess, string operationType, string message)
        {
            IsSuccess = isSuccess;
            OperationType = operationType;
            Message = message;
        }
    }

    public class Result<EType> : ResultBase
    {
        public EType? Data { get; set; }

        private Result(bool isSuccess, EType data, string operationType, string message, int statusCode)
                : base(isSuccess, operationType, message)
        {
            Data = data;
            StatusCode = statusCode;
        }

        public static Result<EType> Success(EType data, string operation,
            string message = "{Operation} operation done successfully", int statusCode = 200)
            => new(true, data, operation, message.Replace("{Operation}", operation), statusCode);

        public static Result<EType> Failure(string operation, string message, int statusCode = 500,
            Dictionary<string, string[]>? errors = null)
            => new(false, default!, operation, message, statusCode) { Errors = errors };

        public static Result<EType> Failure(string operation, string message,
            Dictionary<string, string[]> errors)
            => new(false, default!, operation, message, 400) { Errors = errors };
    }

    public class Result : ResultBase
    {
        private Result(bool isSuccess, string operationType, string message, int statusCode)
        : base(isSuccess, operationType, message)
        {
            StatusCode = statusCode;
        }

        public static Result Success(string operation, string message = "{Operation} operation done successfully", int statusCode = 200)
            => new(true, operation, message.Replace("{Operation}", operation), statusCode);

        public static Result Failure(string operation, string message, int statusCode = 500,
            Dictionary<string, string[]>? errors = null)
            => new(false, operation, message, statusCode) { Errors = errors };

        public static Result Failure(string operation, string message,
            Dictionary<string, string[]> errors)
            => new(false, operation, message, 400) { Errors = errors };
    }
}
