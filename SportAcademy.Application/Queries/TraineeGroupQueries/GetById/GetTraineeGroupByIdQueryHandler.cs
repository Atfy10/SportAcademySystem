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
            // hand-written expression for the card/dropdown queries) - resolve just the Name
            // override with a second small query instead of rebuilding the whole projection.
            var translatedName = await _traineeGroupRepository.GetTranslatedNameAsync(request.Id, _languageProvider.Language, cancellationToken);
            if (translatedName is not null)
                traineeGroup = traineeGroup with { Name = translatedName };

            return Result<TraineeGroupDetailDto>.Success(traineeGroup, _operationType);
        }
    }
}
