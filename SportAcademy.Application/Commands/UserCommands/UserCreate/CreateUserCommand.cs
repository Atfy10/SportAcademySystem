using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.UserCommands.UserCreate
{
    public record CreateUserCommand(
        string UserName,
        string Email,
        string PhoneNumber,
        bool EmailConfirmed = false) : IRequest<Result<string>>, IRequiresFeature, IConsumesLimit
    {
        public string FeatureKey => "user-management";
        public string ResourceKey => LimitedResources.Users;
    }
}
