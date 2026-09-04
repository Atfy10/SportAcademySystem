using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDiscountRequestDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptionDiscountRequestExceptions;

namespace SportAcademy.Application.Queries.SubscriptionDiscountRequestQueries.GetSubscriptionDiscountRequestById;

public class GetSubscriptionDiscountRequestByIdQueryHandler
    : IRequestHandler<GetSubscriptionDiscountRequestByIdQuery, Result<SubscriptionDiscountRequestDto>>
{
    private readonly ISubscriptionDiscountRequestRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetSubscriptionDiscountRequestByIdQueryHandler(
        ISubscriptionDiscountRequestRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<Result<SubscriptionDiscountRequestDto>> Handle(
        GetSubscriptionDiscountRequestByIdQuery request, CancellationToken ct)
    {
        var entity = await _repository.GetByIdWithIncludesAsync(request.Id, ct)
            ?? throw new SubscriptionDiscountRequestNotFoundException(request.Id.ToString());

        var nameLookup = new Dictionary<Guid, string>
        {
            [entity.RequestedByUserId] = await _userRepository.GetDisplayNameAsync(entity.RequestedByUserId, ct),
        };
        if (entity.ReviewedByUserId is { } reviewedBy)
            nameLookup[reviewedBy] = await _userRepository.GetDisplayNameAsync(reviewedBy, ct);

        return Result<SubscriptionDiscountRequestDto>.Success(
            SubscriptionDiscountRequestMapper.ToDto(entity, nameLookup), _operation);
    }
}
