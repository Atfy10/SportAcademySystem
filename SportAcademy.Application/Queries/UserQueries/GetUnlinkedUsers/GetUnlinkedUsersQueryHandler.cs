using MediatR;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;

namespace SportAcademy.Application.Queries.UserQueries.GetUnlinkedUsers
{
    public class GetUnlinkedUsersQueryHandler : IRequestHandler<GetUnlinkedUsersQuery, List<AppUserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly string _operationType = "GetUnlinkedUsers";

        public GetUnlinkedUsersQueryHandler(
            IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<AppUserDto>> Handle(GetUnlinkedUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetUnlinkedUsers(cancellationToken)
                ?? [];

            var usersDto = users.Select(u => u.ToDto()).ToList();

            return usersDto;
        }
    }
}
