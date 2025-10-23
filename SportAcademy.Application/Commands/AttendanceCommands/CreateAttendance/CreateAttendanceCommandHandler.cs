using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance
{
    public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, Result<int>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMapper _mapper;
        private readonly string _operation = OperationType.Add.ToString();

        public CreateAttendanceCommandHandler(
            IAttendanceRepository attendanceRepository,
            IMapper mapper)
        {
            _attendanceRepository = attendanceRepository;
            _mapper = mapper;
        }

        public async Task<Result<int>> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
        {
            var attendanceEntity = _mapper.Map<Attendance>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            cancellationToken.ThrowIfCancellationRequested();

            await _attendanceRepository.AddAsync(attendanceEntity, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(attendanceEntity.Id, _operation);
        }
    }
}
