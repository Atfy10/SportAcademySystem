using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetById
{
    public class GetTraineeGroupByIdQueryHandler : IRequestHandler<GetTraineeGroupByIdQuery, Result<TraineeGroupDetailDto>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly ICurrentLanguageProvider _languageProvider;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetTraineeGroupByIdQueryHandler(ITraineeGroupRepository traineeGroupRepository, ICurrentLanguageProvider languageProvider)
        {
            _traineeGroupRepository = traineeGroupRepository;
            _languageProvider = languageProvider;
        }

        public async Task<Result<TraineeGroupDetailDto>> Handle(GetTraineeGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var traineeGroup = await _traineeGroupRepository.GetDetailsByIdAsync(request.Id, cancellationToken)
                ?? throw new TraineeGroupNotFoundException($"{request.Id}");

            // GetDetailsByIdAsync builds the DTO via AutoMapper's ProjectTo, which can't take a
            // per-request lang closure (same reason TraineeGroupProjections exists as a
            // hand-written expression for the card/dropdown queries) - resolve the Name/
            // SportName/BranchName overrides with a couple of small queries instead of
            // rebuilding the whole projection.
            var translatedName = await _traineeGroupRepository.GetTranslatedNameAsync(request.Id, _languageProvider.Language, cancellationToken);
            var (translatedSportName, translatedBranchName) = await _traineeGroupRepository.GetTranslatedSportBranchNamesAsync(request.Id, _languageProvider.Language, cancellationToken);

            traineeGroup = traineeGroup with
            {
                Name = translatedName ?? traineeGroup.Name,
                SportName = translatedSportName ?? traineeGroup.SportName,
                BranchName = translatedBranchName ?? traineeGroup.BranchName,
            };

            return Result<TraineeGroupDetailDto>.Success(traineeGroup, _operationType);
        }
    }
}
