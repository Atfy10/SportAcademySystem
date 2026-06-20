using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.UserQueries.GetAll
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<AppUserCardDto>>>
    {
        private readonly IUserRepository _userRepository;
        private readonly string _operation = OperationType.GetAll.ToString();

        public GetAllUsersQueryHandler(IUserRepository userRepository)
        {
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
