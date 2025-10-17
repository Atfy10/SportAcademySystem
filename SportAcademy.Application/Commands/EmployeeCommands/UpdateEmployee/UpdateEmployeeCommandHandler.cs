using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee
{
    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result<EmployeeDto>>
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateEmployeeCommandHandler(
            IMapper mapper,
            IEmployeeRepository employeeRepository)
        {
            _mapper = mapper;
            _employeeRepository = employeeRepository;
        }

        public async Task<Result<EmployeeDto>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EmployeeNotFoundException($"{request.Id}");

            _mapper.Map(request, employee);

            cancellationToken.ThrowIfCancellationRequested();

            await _employeeRepository.UpdateAsync(employee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var employeeDto = _mapper.Map<EmployeeDto>(employee)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<EmployeeDto>.Success(employeeDto, _operationType);
        }
    }
}
