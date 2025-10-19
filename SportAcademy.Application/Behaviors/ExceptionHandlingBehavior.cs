using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            catch (Exception ex)
            {
                _logger.LogError("{Message}, Inner Exception: {inner}"
                    , ex.Message, ex.InnerException);
                var responseType = typeof(TResponse);
                if (responseType == typeof(Result))
                {
                    var failureInstance = Result.Failure(request.GetType().Name, "An unexpected error occurred.", 500);
                    return (TResponse)(object)failureInstance;
                }

                if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var genericArguments = responseType.GetGenericArguments()[0];
                    var resultGenericType = typeof(Result<>).MakeGenericType(genericArguments);
                    var failureMethod = responseType.GetMethod("Failure",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (failureMethod != null)
                    {
                        var failureInstance = failureMethod?.Invoke(null, new object[] { genericArguments.Name, ex.Message, 500 });
                        return (TResponse)failureInstance!;
                    }
                }
            }

            throw new InvalidOperationException("Unsupported response type");
        }
    }
}
