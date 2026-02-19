using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetGlobalAttendanceRate;

public record GetGlobalAttendanceRateQuery() : IRequest<Result<int>>;
