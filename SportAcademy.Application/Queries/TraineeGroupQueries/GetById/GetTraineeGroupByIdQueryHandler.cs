using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetById
{
    public class GetTraineeGroupByIdQueryHandler : IRequestHandler<GetTraineeGroupByIdQuery, Result<TraineeGroupDetailDto>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetTraineeGroupByIdQueryHandler(ITraineeGroupRepository traineeGroupRepository)
        {
            _traineeGroupRepository = traineeGroupRepository;
        }

        public async Task<Result<TraineeGroupDetailDto>> Handle(GetTraineeGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var traineeGroup = await _traineeGroupRepository.GetDetailsByIdAsync(request.Id, cancellationToken)
                ?? throw new TraineeGroupNotFoundException($"{request.Id}");

            return Result<TraineeGroupDetailDto>.Success(traineeGroup, _operationType);
        }
    }
}
