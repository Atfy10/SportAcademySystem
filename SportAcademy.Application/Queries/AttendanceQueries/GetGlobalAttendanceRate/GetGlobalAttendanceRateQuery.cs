using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetGlobalAttendanceRate;

public record GetGlobalAttendanceRateQuery(Month Month) : IRequest<Result<int>>;
