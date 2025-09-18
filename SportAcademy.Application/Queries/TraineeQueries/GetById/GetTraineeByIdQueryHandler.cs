using MediatR;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.TraineeQueries.GetById
{
    public class GetTraineeByIdQueryHandler : IRequestHandler<GetTraineeByIdQuery, TraineeDto>
    {
        private readonly ITraineeRepository _traineeRepository;

        public GetTraineeByIdQueryHandler(ITraineeRepository traineeRepository)
        {
            _traineeRepository = traineeRepository;
        }
        public async Task<TraineeDto> Handle(GetTraineeByIdQuery request, CancellationToken cancellationToken)
        {
            var trainee = await _traineeRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new IdNotFoundException("Trainee", request.Id.ToString());
        }
    }
}
