using MediatR;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetAttendanceRate
{
    public class GetAttendanceRateQueryHandler
    : IRequestHandler<GetAttendanceRateQuery, Result<AttendanceRateDto>>
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetAttendanceRateQueryHandler(
            IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        public async Task<Result<AttendanceRateDto>> Handle(
            GetAttendanceRateQuery request,
            CancellationToken cancellationToken)
        {
            var (total, attended) =
                await _attendanceRepository.GetAttendanceSummaryAsync(
                    request.TraineeId,
                    request.FromDate,
                    request.ToDate,
                    cancellationToken);

            double rate = total == 0
                ? 0
                : Math.Round((double)attended / total * 100, 2);

            var dto = new AttendanceRateDto(
                request.TraineeId,
                total,
                attended,
                rate);

            return Result<AttendanceRateDto>.Success(dto, _operationType);
        }
    }

}
