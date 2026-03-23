using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using System.Reflection;
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
                    "Some information you entered is invalid. Please review and try again.",
                    errors);
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

                _logger.LogWarning(ex,
                    "Resource not found for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 404);
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
            catch (AutoMapperMappingException ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogWarning(ex,
                    "AutoMapper mapping failed for {RequestType}. Message: {Message}",
                    requestType,
                    ex.Message);

                return CreateFailure<TResponse>(requestType, ex.Message, 400);
            }
            catch (Exception ex)
            {
                var requestType = request.GetType().Name;

                _logger.LogError(ex,
                    "Unhandled exception occurred for {RequestType}",
                    requestType);

                return CreateFailure<TResponse>(
                    requestType,
                    "An unexpected error occurred. Please try again later.",
                    500);
            }
        }

        private static TResult CreateFailure<TResult>(
            string requestName, string message, int statusCode)
            where TResult : ResultBase
        {
            var responseType = typeof(TResponse);

            if (responseType == typeof(Result))
            {
                return (TResult)(object)Result.Failure(requestName, message, statusCode);
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
                    return (TResult)failureInstance!;
                }
            }

            //if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            //{
            //    var genericType = responseType.GetGenericArguments()[0];
            //    var failureMethod = typeof(Result)
            //        .GetMethod(nameof(Result.Failure),
            //            [typeof(string), typeof(string), typeof(int), typeof(Dictionary<string, string[]>)])
            //        ?.MakeGenericMethod(genericType);

            //    if (failureMethod != null)
            //    {
            //        return (TResult)failureMethod.Invoke(null, new object?[] { requestName, message, statusCode, null })!;
            //    }
            //}

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
                    return (TResult)failureInstance!;
                }
            }

            //if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            //{
            //    var genericType = responseType.GetGenericArguments()[0];
            //    var failureMethod = typeof(Result)
            //        .GetMethod(nameof(Result.Failure),
            //            [typeof(string), typeof(string), typeof(Dictionary<string, string[]>)])
            //        ?.MakeGenericMethod(genericType);

            //    if (failureMethod != null)
            //    {
            //        return (TResult)failureMethod.Invoke(null, new object[] { requestName, message, errors })!;
            //    }
            //}

            throw new InvalidOperationException($"Unsupported response type: {responseType.Name}");
        }
    }
}
