using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.UserQueries.GetAll
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<AppUserDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly string _operation = OperationType.GetAll.ToString();

        public GetAllUsersQueryHandler(IUserRepository userRepository,
            IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async Task<Result<List<AppUserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);

            var usersDto = _mapper.Map<List<AppUserDto>>(users);

            return Result<List<AppUserDto>>.Success(usersDto, _operation);
        }
    }
}
