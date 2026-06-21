using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetRenewInfo
{
    public class GetRenewSubscriptionInfoQueryHandler : IRequestHandler<GetRenewSubscriptionInfoQuery, Result<RenewSubscriptionInfoDto>>
    {
        private readonly string _operation = OperationType.Get.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;

        public GetRenewSubscriptionInfoQueryHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
        }

        public async Task<Result<RenewSubscriptionInfoDto>> Handle(GetRenewSubscriptionInfoQuery request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetFullSubscriptionDetails(request.Id, cancellationToken)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            var dto = new RenewSubscriptionInfoDto
            {
                Id = subDetails.Id,
                TraineeName = $"{subDetails.Trainee?.FirstName} {subDetails.Trainee?.LastName}".Trim(),
                SportName = subDetails.SportPrice?.SportSubscriptionType?.Sport?.Name ?? "",
                BranchName = subDetails.SportPrice?.Branch?.Name ?? "",
                SubscriptionTypeName = subDetails.SportPrice?.SportSubscriptionType?.SubscriptionType?.Name.ToString() ?? "",
                Price = subDetails.SportPrice?.Price ?? 0,
                StartDate = subDetails.StartDate,
                EndDate = subDetails.EndDate,
                TraineeId = subDetails.TraineeId,
                SportId = subDetails.SportPrice?.SportSubscriptionType?.SportId ?? 0,
                BranchId = subDetails.SportPrice?.BranchId ?? 0,
                SubscriptionTypeId = subDetails.SportPrice?.SportSubscriptionType?.SubscriptionTypeId ?? 0
            };

            return Result<RenewSubscriptionInfoDto>.Success(dto, _operation);
        }
    }
}
