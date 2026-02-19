using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetAll
{
    public record GetAllEnrollmentsQuery() : IRequest<Result<List<EnrollmentDto>>>;
}
