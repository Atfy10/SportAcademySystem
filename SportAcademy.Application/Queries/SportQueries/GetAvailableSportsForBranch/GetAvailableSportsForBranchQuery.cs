using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.SportQueries.GetAvailableSportsForBranch
{
    public record GetAvailableSportsForBranchQuery(int branchId) : IRequest<Result<List<SportDto>>>;
}
