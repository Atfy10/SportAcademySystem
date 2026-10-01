using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EventCustomerCommands.UpdateEventCustomer
{
    public record UpdateEventCustomerCommand(
        int Id,
        string FullName,
        string PhoneNumber,
        int NationalityCategoryId,
        string? Notes,
        bool IsActive
    ) : IRequest<Result<EventCustomerDto>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
