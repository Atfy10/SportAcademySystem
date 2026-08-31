using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetRenewInfo
{
    public class GetRenewSubscriptionInfoQueryHandler : IRequestHandler<GetRenewSubscriptionInfoQuery, Result<RenewSubscriptionInfoDto>>
    {
        private readonly string _operation = OperationType.Get.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly ICurrentLanguageProvider _languageProvider;

        public GetRenewSubscriptionInfoQueryHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            ICurrentLanguageProvider languageProvider)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _languageProvider = languageProvider;
        }

        public async Task<Result<RenewSubscriptionInfoDto>> Handle(GetRenewSubscriptionInfoQuery request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetFullSubscriptionDetails(request.Id, cancellationToken)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            var lang = _languageProvider.Language;
            var sport = subDetails.SportPrice?.SportSubscriptionType?.Sport;
            var branch = subDetails.SportPrice?.Branch;

            var dto = new RenewSubscriptionInfoDto
            {
                Id = subDetails.Id,
                TraineeName = $"{subDetails.Trainee?.FirstName} {subDetails.Trainee?.LastName}".Trim(),
                SportName = sport?.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? sport?.Name ?? "",
                BranchName = branch?.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? branch?.Name ?? "",
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
