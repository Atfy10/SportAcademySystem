using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.UserQueries.GetById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly string _operation = OperationType.Get.ToString();

        public GetUserByIdQueryHandler(IUserRepository userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }
        public async Task<Result<AppUserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
                throw new ArgumentOutOfRangeException(nameof(request.Id));

            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new UserNotFoundException();

            var userDto = _mapper.Map<AppUserDto>(user)
                ?? throw new AutoMapperMappingException();

            return Result<AppUserDto>.Success(userDto, _operation);
        }
    }
}
