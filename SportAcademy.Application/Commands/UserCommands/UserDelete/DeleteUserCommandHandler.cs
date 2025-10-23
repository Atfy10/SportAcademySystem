using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Commands.UserCommands.UserDelete
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly string _operation = OperationType.Delete.ToString();

        public DeleteUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new UserNotFoundException();

            cancellationToken.ThrowIfCancellationRequested();

            await _userRepository.DeleteAsync(user, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<bool>.Success(true, _operation);
        }
    }
}
