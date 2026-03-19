using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using DomainValidationException = SportAcademy.Domain.Exceptions.GeneralExceptions.ValidationException;

namespace SportAcademy.Application.Behaviors
{
    public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : ResultBase
    {
        private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

        public ExceptionHandlingBehavior(
            ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {RequestType}", request.GetType().Name);
            try
            {
                return await next(cancellationToken);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning("Validation failed for {RequestType}: {Errors}",
                    request.GetType().Name, ex.Errors);

                var errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return CreateFailureWithErrors<TResponse>(request.GetType().Name,
                    "Some information you entered is invalid. Please review and try again.",
                    errors);
            }
            catch (DomainValidationException ex)
            {
                _logger.LogWarning("Domain validation failed for {RequestType}: {Message}",
                    request.GetType().Name, ex.Message);

                return CreateFailure<TResponse>(request.GetType().Name, ex.Message, 400);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Argument exception for {RequestType}: {Message}",
                    request.GetType().Name, ex.Message);

                return CreateFailure<TResponse>(request.GetType().Name, ex.Message, 400);
            }
            catch (IdNotFoundException ex)
            {
                _logger.LogWarning("Resource not found for {RequestType}: {Message}",
                    request.GetType().Name, ex.Message);

                return CreateFailure<TResponse>(request.GetType().Name, ex.Message, 404);
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning("Conflict for {RequestType}: {Message}",
                    request.GetType().Name, ex.Message);

                return CreateFailure<TResponse>(request.GetType().Name, ex.Message, 409);
            }
            catch (SSNNotUniqueException ex)
            {
                _logger.LogWarning("SSN conflict for {RequestType}: {Message}",
                    request.GetType().Name, ex.Message);

                return CreateFailure<TResponse>(request.GetType().Name, ex.Message, 409);
            }
            catch (PhoneNumberNotUniqueException ex)
            {
                _logger.LogWarning("Phone conflict for {RequestType}: {Message}",
                    request.GetType().Name, ex.Message);

                return CreateFailure<TResponse>(request.GetType().Name, ex.Message, 409);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}, Inner Exception: {inner}",
                    ex.Message, ex.InnerException);

                return CreateFailure<TResponse>(request.GetType().Name,
                    "An unexpected error occurred. Please try again later.", 500);
            }
        }

        private static TResult CreateFailure<TResult>(
            string requestName, string message, int statusCode)
            where TResult : ResultBase
        {
            var responseType = typeof(TResult);

            if (responseType == typeof(Result))
            {
                return (TResult)(object)Result.Failure(requestName, message, statusCode);
            }

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var genericType = responseType.GetGenericArguments()[0];
                var failureMethod = typeof(Result)
                    .GetMethod(nameof(Result.Failure),
                        new[] { typeof(string), typeof(string), typeof(int), typeof(Dictionary<string, string[]>) })
                    ?.MakeGenericMethod(genericType);

                if (failureMethod != null)
                {
                    return (TResult)failureMethod.Invoke(null, new object?[] { requestName, message, statusCode, null })!;
                }
            }

            throw new InvalidOperationException($"Unsupported response type: {responseType.Name}");
        }

        private static TResult CreateFailureWithErrors<TResult>(
            string requestName, string message, Dictionary<string, string[]> errors)
            where TResult : ResultBase
        {
            var responseType = typeof(TResult);

            if (responseType == typeof(Result))
            {
                return (TResult)(object)Result.Failure(requestName, message, errors);
            }

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var genericType = responseType.GetGenericArguments()[0];
                var failureMethod = typeof(Result)
                    .GetMethod(nameof(Result.Failure),
                        new[] { typeof(string), typeof(string), typeof(Dictionary<string, string[]>) })
                    ?.MakeGenericMethod(genericType);

                if (failureMethod != null)
                {
                    return (TResult)failureMethod.Invoke(null, new object[] { requestName, message, errors })!;
                }
            }

            throw new InvalidOperationException($"Unsupported response type: {responseType.Name}");
        }
    }
}
