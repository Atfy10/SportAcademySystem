using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SalaryPaymentQueries.GetSalaryPayments;

public class GetSalaryPaymentsQueryHandler : IRequestHandler<GetSalaryPaymentsQuery, Result<PagedData<SalaryPaymentDto>>>
{
    private readonly ISalaryPaymentRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetSalaryPaymentsQueryHandler(ISalaryPaymentRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<Result<PagedData<SalaryPaymentDto>>> Handle(GetSalaryPaymentsQuery request, CancellationToken ct)
    {
        SalaryPaymentStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<SalaryPaymentStatus>(request.Status, true, out var parsed))
            status = parsed;

        var (items, totalCount) = await _repository.GetPagedAsync(
            request.Page, request.EmployeeId, request.BranchId, status, request.PeriodFrom, request.PeriodTo, ct);

        // Resolve each distinct reviewer/payer id once, not once per row - a payroll page is
        // usually reviewed/paid by the same one or two accountants/admins.
        var distinctUserIds = items
            .SelectMany(sp => new[] { sp.ReviewedByUserId, sp.PaidByUserId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var nameLookup = new Dictionary<Guid, string>();
        foreach (var userId in distinctUserIds)
            nameLookup[userId] = await _userRepository.GetDisplayNameAsync(userId, ct);

        var dtos = items.Select(sp => SalaryPaymentMapper.ToDto(sp, nameLookup)).ToList();

        return Result<PagedData<SalaryPaymentDto>>.Success(new PagedData<SalaryPaymentDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page.Page,
            PageSize = request.Page.PageSize,
        }, _operation);
    }
}
