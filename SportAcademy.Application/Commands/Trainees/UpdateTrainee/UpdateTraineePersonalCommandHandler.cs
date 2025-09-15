using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.Trainees.UpdateTrainee
{
    public class UpdateTraineePersonalCommandHandler : IRequestHandler<UpdateTraineePersonalCommand, Result<UpdateTraineePersonalCommand>>
    {
        private readonly ITraineeService _traineeService;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IMapper _mapper;

        public UpdateTraineePersonalCommandHandler(ITraineeService traineeService,
            ITraineeRepository traineeRepository,
            IMapper mapper)
        {
            _traineeService = traineeService;
            _traineeRepository = traineeRepository;
            _mapper = mapper;
        }

        public async Task<Result<UpdateTraineePersonalCommand>> Handle(UpdateTraineePersonalCommand request, CancellationToken cancellationToken)
        {
            var trainee = await _traineeRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new KeyNotFoundException("Trainee not found");

            

            _mapper.Map(request, trainee);
            await _traineeRepository.UpdateAsync(trainee, cancellationToken);

            return Result<UpdateTraineePersonalCommand>.Success(request, OperationType.Update.ToString());
        }
    }
}
