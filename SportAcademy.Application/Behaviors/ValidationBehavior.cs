using MediatR;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Behaviors
{
    public class ValidationBehavior<IRequest, TResponse> : IPipelineBehavior<IRequest, TResponse>
        where IRequest : notnull, IRequest<TResponse>
        where TResponse : ResultBase
    {
        public Task<TResponse> Handle(IRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
