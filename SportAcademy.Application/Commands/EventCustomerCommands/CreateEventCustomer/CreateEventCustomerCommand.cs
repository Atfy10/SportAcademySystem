using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EventCustomerCommands.CreateEventCustomer
{
    public record CreateEventCustomerCommand(
        string FullName,
        string PhoneNumber,
        int NationalityCategoryId,
        string? Notes
    ) : IRequest<Result<EventCustomerDto>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
