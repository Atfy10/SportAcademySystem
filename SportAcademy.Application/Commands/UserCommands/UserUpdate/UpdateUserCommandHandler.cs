using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using SportAcademy.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.UserCommands.UserUpdate
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<AppUserDto>>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly string _operation = OperationType.Update.ToString();

        public UpdateUserCommandHandler(IUserRepository userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Result<AppUserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken)
                ?? throw new UserNotFoundException();

            _mapper.Map(request, user);

            await _userRepository.UpdateAsync(user, cancellationToken);

            var appUserDto = _mapper.Map<AppUserDto>(user)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<AppUserDto>.Success(appUserDto, _operation);
        }
    }
}