using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Marketing;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetLeads;

public class GetLeadsQueryHandler : IRequestHandler<GetLeadsQuery, Result<PagedData<LeadListItemDto>>>
{
    private readonly IBaseRepository<Lead, Guid> _leadRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetLeadsQueryHandler(IBaseRepository<Lead, Guid> leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<Result<PagedData<LeadListItemDto>>> Handle(GetLeadsQuery request, CancellationToken ct)
    {
        var page = PageRequest.Create(request.Page, request.PageSize);

        // In-memory filter/page rather than a bespoke repository method (the pattern
        // ITenantRepository.GetPagedAsync uses for tenants) - lead volume from a marketing
        // funnel is orders of magnitude smaller than the tenant/trainee tables this codebase
        // is otherwise built to paginate at the database. Revisit if that stops being true.
        var all = await _leadRepository.GetAllAsync(ct);

        IEnumerable<Lead> filtered = all;
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<LeadStatus>(request.Status, true, out var status))
        {
            filtered = filtered.Where(l => l.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            filtered = filtered.Where(l =>
                l.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                l.AcademyName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                l.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                l.PhoneNumber.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = filtered.OrderByDescending(l => l.CreatedAt).ToList();
        var items = ordered.Skip(page.Skip).Take(page.PageSize)
            .Select(l => new LeadListItemDto
            {
                Id = l.Id,
                FullName = l.FullName,
                AcademyName = l.AcademyName,
                Email = l.Email,
                PhoneNumber = l.PhoneNumber,
                City = l.City,
                BranchCount = l.BranchCount,
                SourcePage = l.SourcePage,
                UtmSource = l.UtmSource,
                Status = l.Status.ToString(),
                ConvertedTenantId = l.ConvertedTenantId,
                CreatedAt = l.CreatedAt,
            })
            .ToList();

        var data = new PagedData<LeadListItemDto>
        {
            Items = items,
            TotalCount = ordered.Count,
            Page = page.Page,
            PageSize = page.PageSize,
        };

        return Result<PagedData<LeadListItemDto>>.Success(data, _operation);
    }
}
