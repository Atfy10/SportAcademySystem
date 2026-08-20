using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetGlobalAttendanceRate;

public record GetGlobalAttendanceRateQuery(Month? Month, int? Year = null) : IRequest<Result<int>>;
