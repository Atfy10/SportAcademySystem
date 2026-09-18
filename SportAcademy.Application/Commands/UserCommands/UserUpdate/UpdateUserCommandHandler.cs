using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Commands.UserCommands.UserUpdate
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<AppUserDto>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IPhoneNumberNormalizer _phoneNormalizer;
        private readonly string _operation = OperationType.Update.ToString();

        public UpdateUserCommandHandler(IUserRepository userRepository,
            IMapper mapper,
            IPhoneNumberNormalizer phoneNormalizer)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _phoneNormalizer = phoneNormalizer;
        }

        public async Task<Result<AppUserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken)
                ?? throw new UserNotFoundException();

            _mapper.Map(request, user);

            // Normalized to E.164 after the map - see the matching comment in
            // CreateEmployeeCommandHandler.
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                user.PhoneNumber = await _phoneNormalizer.NormalizeAsync(user.PhoneNumber, cancellationToken);

            await _userRepository.UpdateAsync(user, cancellationToken);

            var appUserDto = _mapper.Map<AppUserDto>(user)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<AppUserDto>.Success(appUserDto, _operation);
        }
    }
}