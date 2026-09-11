using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdateLead;

// SuperAdmin triage: change a lead's status and/or leave an internal note. Converting a lead
// into a real tenant stays a separate flow (the existing CreateTenantCommand, pre-filled from
// this lead's data by the console) that then calls this command's status transition to
// Converted with ConvertedTenantId set - see LeadsPage.tsx "Create tenant from this lead".
public record UpdateLeadCommand(
    Guid LeadId,
    string Status,
    string? InternalNotes,
    Guid? ConvertedTenantId
) : IRequest<Result<LeadDetailDto>>;
