using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.UserQueries.GetAll
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<AppUserCardDto>>>
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

        public async Task<Result<List<AppUserCardDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken)
                ?? [];

            var usersDto = new List<AppUserCardDto>();

            foreach (var user in users)
            {
                var roles = await _userRepository.GetUserRoleAsync(user, cancellationToken)
                    ?? [];

                usersDto.Add(new AppUserCardDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Roles = (List<string>)(roles ?? []),
                    IsActive = !user.IsBanned
                });
            }

            return Result<List<AppUserCardDto>>.Success(usersDto, nameof(GetAllUsersQuery));
        }
    }
}
