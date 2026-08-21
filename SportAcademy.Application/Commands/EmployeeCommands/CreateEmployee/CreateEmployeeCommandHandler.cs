using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<CreateEmployeeResultDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly IPersonService _employeeService;
        private readonly string _operationType = OperationType.Add.ToString();
        private readonly IUserRepository _userRepository;

        public CreateEmployeeCommandHandler(
            IPersonService employeeService,
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository)
        {
            _mapper = mapper;
            _employeeService = employeeService;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<CreateEmployeeResultDto>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = _mapper.Map<Employee>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            var isSSNValid = _employeeService.IsSSNValid(employee.SSN, employee.BirthDate);

            if (!isSSNValid)
                throw new SSNSyntaxErrorException();

            var isSSNExist = await _employeeRepository
                .IsSSNExistAsync(employee.SSN, cancellationToken);

            var isPhoneNumberExist = await _employeeRepository
                .IsPhoneNumberExistAsync(employee.PhoneNumber, cancellationToken: cancellationToken);
             if (isPhoneNumberExist)
                throw new PhoneNumberNotUniqueException();

            if (isSSNExist)
                throw new SSNNotUniqueException();

            cancellationToken.ThrowIfCancellationRequested();

            string? generatedUserName = null;
            string? generatedPassword = null;

            if (request.CreateUserAccount)
            {
                generatedUserName = _employeeService.GenerateUserName(employee.FirstName, employee.LastName);
                generatedPassword = _employeeService.GeneratePassword();
                var user = await _userRepository.Register(new AppUser() { UserName = generatedUserName }, generatedPassword);

                if (!user.Succeeded)
                    throw new UserRegistrationException([.. user.Errors.Select(e => e.Description)]);

                employee.AppUser = await _userRepository.GetByUsernameAsync(generatedUserName);
            }

            await _employeeRepository.AddAsync(employee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var resultDto = new CreateEmployeeResultDto(employee.Id, generatedUserName, generatedPassword);
            return Result<CreateEmployeeResultDto>.Success(resultDto, _operationType);
        }
    }
}
