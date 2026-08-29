using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;
using SportAcademy.Domain.Exceptions.PaymentTypeExceptions;
using SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;
using System.Diagnostics;
using System.Reflection;
using DomainValidationException = SportAcademy.Domain.Exceptions.GeneralExceptions.ValidationException;

namespace SportAcademy.Application.Behaviors
{
    public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : ResultBase
    {
        private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;
        private readonly ILocalizationService _localizer;

        public ExceptionHandlingBehavior(
            ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger,
            ILocalizationService localizer)
        {
            _logger = logger;
            _localizer = localizer;
        }

        /// <summary>
        /// Short correlation reference surfaced to the user alongside a generic message, so a
        /// support report can be tied back to the structured log entry for the same request.
        /// </summary>
        private static string CurrentTraceId() =>
            Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {RequestType}", request.GetType().Name);
            try
            {
                return await next(cancellationToken);
            }
            catch (ValidationException ex)
            {
                var requestType = request.GetType().Name;

                var errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                _logger.LogWarning(ex,
                    "Validation failed for {RequestType}. Errors: {@Errors}",
                    requestType,
                    errors);

                return CreateFailureWithErrors<TResponse>(
                    requestType,
                    _localizer["errors.validation.failed"],
                    errors,
                    "errors.validation.failed");
            }
            catch (DomainValidationException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Domain validation failed for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 400);
            }
            catch (ArgumentException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Argument exception for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 400);
            }
            catch (IdNotFoundException ex)
            {
                var requestType = request.GetType().Name;

                // Structured properties, not a pre-baked sentence, so logs stay filterable.
                _logger.LogInformation(ex,
                    "Resource not found for {RequestType}. Entity {EntityType} Id {EntityId}",
                    requestType,
                    ex.Entity,
                    ex.Id);

                // The user is told what is missing, not which table row: ids in error text are
                // noise to them and mild information disclosure in a multi-tenant product.
                var entityKey = "entity." + ex.Entity;
                var entityName = _localizer.Exists(entityKey) ? _localizer[entityKey] : ex.Entity;

                return CreateFailure<TResponse>(
                    requestType,
                    _localizer["errors.notFound", entityName],
                    404,
                    "errors.notFound");
            }
            catch (ConflictException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Conflict detected for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (SSNNotUniqueException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "SSN conflict for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (PhoneNumberNotUniqueException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Phone number conflict for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (InvalidSearchTermException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Invalid search term for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 400);
            }
            catch (InvalidDurationException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Invalid duration for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 400);
            }
            catch (NoSchedulesFoundException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "No schedules found for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (SessionGapTooLargeException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Session gap too large for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (GroupAtCapacityException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Trainee group at capacity for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (TraineeAlreadyEnrolledInSportException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Trainee already enrolled in this sport for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (EnrollmentGroupSportMismatchException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Enrollment/group sport mismatch for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (SubscriptionGroupSportMismatchException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Subscription/group sport mismatch for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (TraineeGenderMismatchException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Trainee/group gender mismatch for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (TraineeSkillLevelTooLowException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Trainee skill level too low for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (CoachSkillLevelTooLowException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Coach skill level too low for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (CoachSportMismatchException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Coach sport mismatch for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (PaymentTypeInUseException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Payment type in use for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (NoDefaultPaymentTypeException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "No default payment type configured for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 409);
            }
            catch (AutoMapperMappingException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "AutoMapper mapping failed for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 400);
            }
            catch (UserLoginException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Login failed for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 400);
            }
            // Migration target: exceptions adopt LocalizableException a few at a time. Anything
            // not yet migrated falls through to its specific catch above and keeps emitting the
            // literal English it always did, so this never needs to be a big-bang rewrite.
            catch (LocalizableException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "Domain rule rejected {RequestType}. Code {ErrorCode}",
                    requestType,
                    ex.MessageKey);

                var message = _localizer.Exists(ex.MessageKey)
                    ? _localizer[ex.MessageKey, ex.Args]
                    : ex.Message;

                return CreateFailure<TResponse>(requestType, message, 400, ex.MessageKey);
            }
            catch (Exception ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogError(ex,
                    "Unhandled exception occurred for {RequestType}",
                    requestType);

                // The user gets a generic sentence plus a reference; the exception itself stays in
                // the log, where LogError above has already recorded it with full context.
                return CreateFailure<TResponse>(
                    requestType,
                    _localizer["errors.generic.withReference", CurrentTraceId()],
                    500,
                    "errors.generic");
            }
        }

        /// <summary>Stamps the machine-readable code and the correlation reference onto a failure.</summary>
        private static TResult Stamp<TResult>(TResult result, string? code)
            where TResult : ResultBase
        {
            result.Code = code;
            result.TraceId = CurrentTraceId();
            return result;
        }

        private static TResult CreateFailure<TResult>(
            string requestName, string message, int statusCode, string? code = null)
            where TResult : ResultBase
        {
            var responseType = typeof(TResponse);

            if (responseType == typeof(Result))
            {
                return Stamp((TResult)(object)Result.Failure(requestName, message, statusCode), code);
            }

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var genericArguments = responseType.GetGenericArguments()[0];
                var resultGenericType = typeof(Result<>).MakeGenericType(genericArguments);
                var failureMethod = resultGenericType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                        m.Name == "Failure" &&
                        m.GetParameters().Length == 4 &&
                        m.GetParameters()[3].ParameterType == typeof(Dictionary<string, string[]>));

                if (failureMethod != null)
                {
                    var failureInstance = failureMethod?.Invoke(null, new object?[] { requestName, message, statusCode, null });
                    return Stamp((TResult)failureInstance!, code);
                }
            }

            throw new InvalidOperationException($"Unsupported response type: {responseType.Name}");
        }

        private static TResult CreateFailureWithErrors<TResult>(
            string requestName, string message, Dictionary<string, string[]> errors, string? code = null)
            where TResult : ResultBase
        {
            var responseType = typeof(TResult);

            if (responseType == typeof(Result))
            {
                return Stamp((TResult)(object)Result.Failure(requestName, message, errors), code);
            }

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var genericArguments = responseType.GetGenericArguments()[0];
                var resultGenericType = typeof(Result<>).MakeGenericType(genericArguments);
                var failureMethod = resultGenericType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                        m.Name == "Failure" &&
                        m.GetParameters().Length == 3 &&
                        m.GetParameters()[2].ParameterType == typeof(Dictionary<string, string[]>));
                if (failureMethod != null)
                {
                    var failureInstance = failureMethod?.Invoke(null, new object[] { requestName, message, errors });
                    return Stamp((TResult)failureInstance!, code);
                }
            }

            throw new InvalidOperationException($"Unsupported response type: {responseType.Name}");
        }
    }
}
