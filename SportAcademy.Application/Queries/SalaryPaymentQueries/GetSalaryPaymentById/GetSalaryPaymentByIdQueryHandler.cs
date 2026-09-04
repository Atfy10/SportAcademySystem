using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SalaryPaymentExceptions;

namespace SportAcademy.Application.Queries.SalaryPaymentQueries.GetSalaryPaymentById;

public class GetSalaryPaymentByIdQueryHandler : IRequestHandler<GetSalaryPaymentByIdQuery, Result<SalaryPaymentDto>>
{
    private readonly ISalaryPaymentRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetSalaryPaymentByIdQueryHandler(ISalaryPaymentRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<Result<SalaryPaymentDto>> Handle(GetSalaryPaymentByIdQuery request, CancellationToken ct)
    {
        var entity = await _repository.GetByIdWithIncludesAsync(request.Id, ct)
            ?? throw new SalaryPaymentNotFoundException(request.Id.ToString());

        var nameLookup = new Dictionary<Guid, string>();
        if (entity.ReviewedByUserId is { } reviewedBy)
            nameLookup[reviewedBy] = await _userRepository.GetDisplayNameAsync(reviewedBy, ct);
        if (entity.PaidByUserId is { } paidBy)
            nameLookup[paidBy] = await _userRepository.GetDisplayNameAsync(paidBy, ct);

        return Result<SalaryPaymentDto>.Success(SalaryPaymentMapper.ToDto(entity, nameLookup), _operation);
    }
}
