using MediatR;
using SportAcademy.Application.Common.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.BranchCommands.AddSportToBranch
{
    public record AddSportToBranchCommand(
          int SportId,
          int BranchId
      ) : IRequest<Result<string>>;
}
