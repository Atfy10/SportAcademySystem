using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.UserCommands.UserCreate
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<string>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateUserCommandHandler(IUserRepository userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var user = _mapper.Map<AppUser>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            await _userRepository.AddAsync(user, cancellationToken);
            return Result<string>.Success(user.Id, _operationType);
        }
    }
}
