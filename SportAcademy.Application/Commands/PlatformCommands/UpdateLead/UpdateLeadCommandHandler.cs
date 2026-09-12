using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Marketing;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdateLead;

public class UpdateLeadCommandHandler : IRequestHandler<UpdateLeadCommand, Result<LeadDetailDto>>
{
    private readonly IBaseRepository<Lead, Guid> _leadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateLeadCommandHandler(IBaseRepository<Lead, Guid> leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LeadDetailDto>> Handle(UpdateLeadCommand request, CancellationToken ct)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, ct);
        if (lead is null)
            return Result<LeadDetailDto>.Failure(_operation, "Lead not found.", 404);

        if (!Enum.TryParse<LeadStatus>(request.Status, true, out var status))
            return Result<LeadDetailDto>.Failure(_operation, $"Unknown lead status '{request.Status}'.", 400);

        var wasContacted = lead.Status != LeadStatus.New;

        lead.Status = status;
        lead.InternalNotes = request.InternalNotes;
        if (request.ConvertedTenantId.HasValue)
            lead.ConvertedTenantId = request.ConvertedTenantId;

        if (!wasContacted && status != LeadStatus.New && lead.ContactedAt is null)
            lead.ContactedAt = DateTime.UtcNow;

        await _leadRepository.UpdateAsync(lead, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = new LeadDetailDto
        {
            Id = lead.Id,
            FullName = lead.FullName,
            AcademyName = lead.AcademyName,
            Email = lead.Email,
            PhoneNumber = lead.PhoneNumber,
            City = lead.City,
            BranchCount = lead.BranchCount,
            TraineeCountBand = lead.TraineeCountBand,
            Message = lead.Message,
            Locale = lead.Locale,
            SourcePage = lead.SourcePage,
            UtmSource = lead.UtmSource,
            UtmMedium = lead.UtmMedium,
            UtmCampaign = lead.UtmCampaign,
            Referrer = lead.Referrer,
            Status = lead.Status.ToString(),
            InternalNotes = lead.InternalNotes,
            ConvertedTenantId = lead.ConvertedTenantId,
            CreatedAt = lead.CreatedAt,
            ContactedAt = lead.ContactedAt,
        };

        return Result<LeadDetailDto>.Success(dto, _operation, "Lead updated.");
    }
}
