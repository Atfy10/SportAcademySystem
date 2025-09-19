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
    public class ExceptionHandlingBehavior<IRequest, TResponse> : IPipelineBehavior<IRequest, TResponse>
        where IRequest : notnull, IRequest<TResponse>
        where TResponse : ResultBase
    {
        private readonly ILogger<ExceptionHandlingBehavior<IRequest, TResponse>> _logger;

        public ExceptionHandlingBehavior(
            ILogger<ExceptionHandlingBehavior<IRequest, TResponse>> logger)
        {
            _logger = logger;
        }
        public Task<TResponse> Handle(IRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {RequestType}", request.GetType().Name);
            throw new NotImplementedException();
        }
    }
}
