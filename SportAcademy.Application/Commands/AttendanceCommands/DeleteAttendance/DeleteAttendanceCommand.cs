﻿using MediatR;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.AttendanceCommands.DeleteAttendance
{
    public record DeleteAttendanceCommand(int Id) : IRequest<Result<bool>>;
}
