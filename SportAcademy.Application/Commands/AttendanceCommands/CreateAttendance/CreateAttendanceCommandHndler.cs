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

namespace SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance
{
    public class CreateAttendanceCommandHndler : IRequestHandler<CreateAttendanceCommand, Result<int>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IAttendanceService _attendanceService;
        private readonly IMapper _mapper;
        private readonly string _operation = OperationType.Add.ToString();

        public CreateAttendanceCommandHndler(IAttendanceRepository attendanceRepository,
            IMapper mapper,
            IAttendanceService attendanceService,
            IEnrollmentRepository enrollmentRepository)
        {
            _attendanceRepository = attendanceRepository;
            _enrollmentRepository = enrollmentRepository;
            _attendanceService = attendanceService;
            _mapper = mapper;
        }
        public async Task<Result<int>> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
        {
            var attendance = _mapper.Map<Attendance>(request);

            cancellationToken.ThrowIfCancellationRequested();

            var enrollment = await _enrollmentRepository.GetEnrollmentWithSession(request.EnrollmentId!.Value, cancellationToken);
            var sessionDate = enrollment?.Session.Date;

            var isSessionDate = _attendanceService.IsSessionDate(DateOnly.FromDateTime(DateTime.Now), sessionDate!.Value);
            if (!isSessionDate)
                throw new SessionDateMismatchException();

            attendance.CheckInTime = TimeOnly.FromDateTime(DateTime.Now);
            attendance.AttendanceDate = DateTime.Now;

            cancellationToken.ThrowIfCancellationRequested();

            await _attendanceRepository.AddAsync(attendance, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<int>.Success(1, _operation);
        }
    }
}
