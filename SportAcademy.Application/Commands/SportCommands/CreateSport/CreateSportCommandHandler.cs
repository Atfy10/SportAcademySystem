using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.SportCommands.CreateSport
{
    public class CreateSportCommandHandler : IRequestHandler<CreateSportCommand, Result<int>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateSportCommandHandler(
            ISportRepository sportRepository,
            IMapper mapper)
        {
            _sportRepository = sportRepository;
            _mapper = mapper;
        }

        public async Task<Result<int>> Handle(CreateSportCommand request, CancellationToken cancellationToken)
        {
            var sport = _mapper.Map<Sport>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            var nameExists = await _sportRepository.IsExistByNameAsync(sport.Name, cancellationToken);
            if (nameExists)
                throw new SportExistsException();

            cancellationToken.ThrowIfCancellationRequested();

            await _sportRepository.AddAsync(sport, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(sport.Id, _operationType);
        }
    }
}
