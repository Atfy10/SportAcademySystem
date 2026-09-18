using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
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
        private readonly IPhoneNumberNormalizer _phoneNormalizer;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateUserCommandHandler(IUserRepository userRepository,
            IMapper mapper,
            IPhoneNumberNormalizer phoneNormalizer)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _phoneNormalizer = phoneNormalizer;
        }

        public async Task<Result<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var user = _mapper.Map<AppUser>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            // Normalized to E.164 before save - see the matching comment in
            // CreateEmployeeCommandHandler.
            user.PhoneNumber = await _phoneNormalizer.NormalizeAsync(user.PhoneNumber, cancellationToken)
                ?? user.PhoneNumber;

            await _userRepository.AddAsync(user, cancellationToken);
            return Result<string>.Success(user.Id.ToString(), _operationType);
        }
    }
}
