using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetBySessionOccurrence;

public record GetAttendanceBySessionOccurrenceQuery(int SessionOccurrenceId) : IRequest<Result<List<AttendanceRecordDto>>>;
