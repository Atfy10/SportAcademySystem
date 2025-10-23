using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.SportQueries.GetAll
{
    public class GetAllSportsQueryHandler : IRequestHandler<GetAllSportsQuery, Result<List<SportDto>>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllSportsQueryHandler(
            ISportRepository sportRepository,
            IMapper mapper)
        {
            _sportRepository = sportRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<SportDto>>> Handle(GetAllSportsQuery request, CancellationToken cancellationToken)
        {
            var sports = await _sportRepository.GetAllAsync(cancellationToken) ?? [];

            var sportsDto = _mapper.Map<List<SportDto>>(sports) ?? [];

            return Result<List<SportDto>>.Success(sportsDto, _operationType);
        }
    }
}
