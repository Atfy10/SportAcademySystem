using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.DiscountCodeCommands.DeleteDiscountCode
{
    public record DeleteDiscountCodeCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
    {
        public string FeatureKey => "discount-offers";
    }
}
