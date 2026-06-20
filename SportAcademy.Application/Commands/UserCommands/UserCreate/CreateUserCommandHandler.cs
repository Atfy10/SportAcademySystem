using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.UserCommands.UserCreate
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var user = request.ToAppUser();
            await _userRepository.AddAsync(user, cancellationToken);
            return Result<string>.Success(user.Id, _operationType);
        }
    }
}
