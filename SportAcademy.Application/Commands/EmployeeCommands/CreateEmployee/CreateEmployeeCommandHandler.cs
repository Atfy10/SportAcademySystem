using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<int>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly IEmployeeService _employeeService;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateEmployeeCommandHandler(
            IEmployeeService employeeService,
            IMapper mapper,
            IEmployeeRepository employeeRepository)
        {
            _mapper = mapper;
            _employeeService = employeeService;
            _employeeRepository = employeeRepository;
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

            await _employeeRepository.AddAsync(employee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(employee.Id, _operationType);
        }
    }
}
