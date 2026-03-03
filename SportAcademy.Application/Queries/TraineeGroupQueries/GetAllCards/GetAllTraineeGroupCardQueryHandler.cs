using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAllCards
{
    public class GetAllTraineeGroupCardQueryHandler: IRequestHandler<GetAllTraineeGroupCardQuery, Result<PagedData<TraineeGroupCardDto>>>
    {
        private readonly ITraineeGroupRepository _traineeGroupRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.GetAll.ToString();
        public GetAllTraineeGroupCardQueryHandler(ITraineeGroupRepository traineeGroupRepository, IMapper mapper)
        {
            _traineeGroupRepository = traineeGroupRepository;
            _mapper = mapper;
        }
        public async Task<Result<PagedData<TraineeGroupCardDto>>> Handle(
      GetAllTraineeGroupCardQuery request,
      CancellationToken cancellationToken)
        {
            var pagedData = await _traineeGroupRepository.GetAllAsync(request.Page, cancellationToken);

            return Result<PagedData<TraineeGroupCardDto>>
                .Success(pagedData, _operationType);
        }

    }
}
