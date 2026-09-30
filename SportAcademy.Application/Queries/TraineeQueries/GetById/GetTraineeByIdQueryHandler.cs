using AutoMapper;
using MediatR;
using SportAcademy.Domain.Services;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeExceptions;

namespace SportAcademy.Application.Queries.TraineeQueries.GetById
{
    public class GetTraineeByIdQueryHandler : IRequestHandler<GetTraineeByIdQuery, Result<TraineeDetailsDto>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetTraineeByIdQueryHandler(
            IAttendanceRepository attendanceRepository,
            ITraineeRepository traineeRepository,
            IMapper mapper)
        {
            _attendanceRepository = attendanceRepository;
            _traineeRepository = traineeRepository;
            _mapper = mapper;
        }
        public async Task<Result<TraineeDetailsDto>> Handle(GetTraineeByIdQuery request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                throw new ArgumentException("Id must be greater than zero.");

            cancellationToken.ThrowIfCancellationRequested();

            var trainee = await _traineeRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new TraineeNotFoundException(request.Id.ToString());

            // This trainee's own attendance (it was hard-coded to trainee 15 for everyone).
            (int totalSessions, int attendedSessions) = await _attendanceRepository.GetAttendanceSummaryAsync(request.Id, null, null, cancellationToken);
            trainee.AttendanceRate = AttendanceRate.Percent(attendedSessions, totalSessions);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<TraineeDetailsDto>.Success(trainee, _operationType);
        }
    }
}
