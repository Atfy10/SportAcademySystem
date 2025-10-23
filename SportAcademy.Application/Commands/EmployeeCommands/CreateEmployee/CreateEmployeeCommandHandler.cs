using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SharedExceptions;

namespace SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<int>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly IEmployeeService _employeeService;
        private readonly string _operationType = OperationType.Add.ToString();
        private readonly IUserRepository _userRepository;

        public CreateEmployeeCommandHandler(
            IEmployeeService employeeService,
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository)
        {
            _mapper = mapper;
            _employeeService = employeeService;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<int>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = _mapper.Map<Employee>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            var isSSNValid = _employeeService.IsSSNValid(employee.SSN, employee.BirthDate);

            if (!isSSNValid)
                throw new SSNSyntaxErrorException();

            var isSSNExist = await _employeeRepository
                .IsSSNExistAsync(employee.SSN, cancellationToken);

            if (isSSNExist)
                throw new SSNNotUniqueException();

            cancellationToken.ThrowIfCancellationRequested();

            var users = await _userRepository.GetAllAsync();

            employee.AppUserId = users[Random.Shared.Next(0, 50)].Id;

            await _employeeRepository.AddAsync(employee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(employee.Id, _operationType);
        }
    }
}
