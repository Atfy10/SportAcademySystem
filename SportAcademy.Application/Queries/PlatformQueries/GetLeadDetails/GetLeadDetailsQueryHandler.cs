using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Marketing;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetLeadDetails;

public class GetLeadDetailsQueryHandler : IRequestHandler<GetLeadDetailsQuery, Result<LeadDetailDto>>
{
    private readonly IBaseRepository<Lead, Guid> _leadRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetLeadDetailsQueryHandler(IBaseRepository<Lead, Guid> leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<Result<LeadDetailDto>> Handle(GetLeadDetailsQuery request, CancellationToken ct)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, ct);
        if (lead is null)
            return Result<LeadDetailDto>.Failure(_operation, "Lead not found.", 404);

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

        return Result<LeadDetailDto>.Success(dto, _operation);
    }
}
