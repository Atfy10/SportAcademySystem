using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Commands.UserCommands.UserUpdate
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<AppUserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly string _operation = OperationType.Update.ToString();

        public UpdateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<AppUserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken)
                ?? throw new UserNotFoundException();

            user.ApplyUpdate(request);

            await _userRepository.UpdateAsync(user, cancellationToken);

            var appUserDto = user.ToDto();

            return Result<AppUserDto>.Success(appUserDto, _operation);
        }
    }
}