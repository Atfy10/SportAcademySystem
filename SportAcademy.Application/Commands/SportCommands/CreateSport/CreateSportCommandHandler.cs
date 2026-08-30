using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Translations;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using SportAcademy.Domain.Exceptions.SportExceptions;

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

            // The Arabic translation is attached to the nav collection - not saved yet - so EF
            // cascade-inserts it in the same AddAsync/SaveChanges call as the sport itself,
            // the same idiom CreateTraineeGroupCommandHandler uses for GroupSchedules.
            if (!string.IsNullOrWhiteSpace(request.NameAr))
            {
                sport.Translations = new List<SportTranslation>
                {
                    new()
                    {
                        LangCode = "ar",
                        Name = request.NameAr.Trim(),
                        Description = string.IsNullOrWhiteSpace(request.DescriptionAr)
                            ? null
                            : request.DescriptionAr.Trim(),
                    }
                };
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _sportRepository.AddAsync(sport, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(sport.Id, _operationType);
        }
    }
}
