using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<int>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Add.ToString();
        private readonly IUserRepository _userRepository;

        public CreateEmployeeCommandHandler(
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository)
        {
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<int>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = _mapper.Map<Employee>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            var isSSNExist = await _employeeRepository
                .IsSSNExistAsync(employee.SSN, cancellationToken);

            var isPhoneNumberExist = await _employeeRepository
                .IsPhoneNumberExistAsync(employee.PhoneNumber, cancellationToken: cancellationToken);
             if (isPhoneNumberExist)
                throw new PhoneNumberNotUniqueException();

            if (isSSNExist)
                throw new SSNNotUniqueException();

            cancellationToken.ThrowIfCancellationRequested();

            var userName = GenerateUserName(employee.FirstName, employee.LastName);
            var password = GeneratePassword();
            var user = await _userRepository.Register(AppUser.CreateForEmployee(userName), password);

            if(!user.Succeeded)
                throw new UserRegistrationException([.. user.Errors.Select(e => e.Description)]);

            employee.AppUser = await _userRepository.GetByUsernameAsync(userName);

            await _employeeRepository.AddAsync(employee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(employee.Id, _operationType);
        }

        private static string GenerateUserName(string firstName, string lastName)
        {
            var userName = $"{firstName.ToLower().Trim()}{lastName.ToLower().Trim()[..2]}_{Random.Shared.Next(0, 50):D2}";
            return userName;
        }

        private static string GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var password = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
            return password;
        }
    }
}
