using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;
using System.Collections.Generic;

namespace SportAcademy.Application.Commands.AttendanceCommands.BulkCreateAttendance;

public record BulkCreateAttendanceCommand(List<AttendanceItem> Items) : IRequest<Result<bool>>;

public record AttendanceItem(
    int SessionOccurrenceId,
    int TraineeId,
    AttendanceStatus Status,
    string? CheckInTime
);
