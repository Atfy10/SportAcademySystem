using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;

namespace SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee
{
    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result<EmployeeDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateEmployeeCommandHandler(
            IEmployeeRepository employeeRepository,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
        }

        public async Task<Result<EmployeeDto>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EmployeeNotFoundException($"{request.Id}");

            if (request.PhoneNumber != null && request.PhoneNumber != employee.PhoneNumber)
            {
                var isPhoneNumberExist = await _employeeRepository
                    .IsPhoneNumberExistAsync(request.PhoneNumber, employee.Id, cancellationToken);
                if (isPhoneNumberExist)
                    throw new PhoneNumberNotUniqueException();
            }

            if (request.ImageUrl != null && request.ImageUrl != employee.ImageUrl)
                _fileStorage.DeleteImage(employee.ImageUrl);

            EmployeeMapper.ApplyUpdate(employee, request);

            cancellationToken.ThrowIfCancellationRequested();

            await _employeeRepository.UpdateAsyncWithoutSave(employee, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<EmployeeDto>.Success(EmployeeMapper.ToDto(employee), _operationType);
        }
    }
}
