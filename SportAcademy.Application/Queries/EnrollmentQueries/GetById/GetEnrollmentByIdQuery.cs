using MediatR;
using SportAcademy.Application.DTOs.EnrollmentDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetById
{
    public record GetEnrollmentByIdQuery(int Id) : IRequest<Result<EnrollmentDto>>;
}
