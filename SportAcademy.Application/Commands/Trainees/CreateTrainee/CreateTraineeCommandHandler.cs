using AutoMapper;
using MediatR;
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

namespace SportAcademy.Application.Commands.Trainees.CreateTrainee
{
    public class CreateTraineeCommandHandler : IRequestHandler<CreateTraineeCommand, Result<int>>
    {
        private readonly IMapper _mapper;
        private readonly ITraineeService _traineeService;
        private readonly ITraineeRepository _traineeRepository;

        public CreateTraineeCommandHandler(ITraineeService traineeService,
            IMapper mapper,
            ITraineeRepository traineeRepository)
        {
            _mapper = mapper;
            _traineeService = traineeService;
            _traineeRepository = traineeRepository;
        }

        public async Task<Result<int>> Handle(CreateTraineeCommand request, CancellationToken cancellationToken)
        {
            var trainee = _mapper.Map<Trainee>(request);

            if (!_traineeService.IsSSNValid(trainee.SSN, trainee.BirthDate))
                throw new SSNSyntaxErrorException();

            var isSSNExist = await _traineeRepository
                .IsSSNExistAsync(trainee.SSN, cancellationToken);
            if (isSSNExist)
                throw new SSNNotUniqueException();

            trainee.Id = _traineeService.CreateTraineeCode(trainee, request.BranchId);

            bool isAdult = _traineeService.IsAdult(trainee.BirthDate);
            if (!isAdult && (string.IsNullOrEmpty(trainee.ParentNumber)
                || string.IsNullOrEmpty(trainee.GuardianName)))
                throw new GuardianInfoMissingException();

            if (isAdult)
            {
                trainee.ParentNumber = null;
                trainee.GuardianName = null;
            }

            trainee.IsSubscribed = false;

            await _traineeRepository.AddAsync(trainee, cancellationToken);
            return Result<int>.Success(trainee.Id, OperationType.Add.ToString());
        }
    }
}