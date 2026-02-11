using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.UserQueries.GetUnlinkedUsers
{
    public class GetUnlinkedUsersQueryHandler : IRequestHandler<GetUnlinkedUsersQuery, List<AppUserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = "GetUnlinkedUsers";

        public GetUnlinkedUsersQueryHandler(
            IUserRepository userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<List<AppUserDto>> Handle(GetUnlinkedUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetUnlinkedUsers(cancellationToken)
                ?? [];

            var usersDto = _mapper.Map<List<AppUserDto>>(users)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return usersDto;
        }
    }
}
