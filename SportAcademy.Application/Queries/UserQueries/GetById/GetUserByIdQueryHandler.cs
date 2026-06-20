using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Queries.UserQueries.GetById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly string _operation = OperationType.Get.ToString();

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<Result<AppUserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
                throw new ArgumentOutOfRangeException(nameof(request.Id));

            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new UserNotFoundException();

            var userDto = user.ToDto();

            return Result<AppUserDto>.Success(userDto, _operation);
        }
    }
}
