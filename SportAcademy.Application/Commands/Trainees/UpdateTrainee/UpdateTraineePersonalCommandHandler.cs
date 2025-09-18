using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using SportAcademy.Domain.Extensions;
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
        private readonly string _operationType = OperationType.Update.ToString();

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
            var trainee = await _traineeRepository.GetFullTrainee(request.Id, cancellationToken)
                ?? throw new IdNotFoundException(EntityTypes.Trainee.DisplayName(),
                                request.Id.ToString());

            _mapper.Map(request, trainee);

            await _traineeRepository.UpdateAsync(trainee, cancellationToken);

            return Result<UpdateTraineePersonalCommand>.Success(request, _operationType);
        }
    }
}
