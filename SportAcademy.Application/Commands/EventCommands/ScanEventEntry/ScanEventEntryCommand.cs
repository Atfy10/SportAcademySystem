using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;

namespace SportAcademy.Application.Commands.EventCommands.ScanEventEntry
{
    // Sent by the public entry page when a guest scans the event's QR code. Anonymous: Token
    // identifies the event, DeviceKey the phone (a random id the page keeps in the browser).
    public record ScanEventEntryCommand(string Token, string DeviceKey) : IRequest<Result<EventEntryResultDto>>;
}
