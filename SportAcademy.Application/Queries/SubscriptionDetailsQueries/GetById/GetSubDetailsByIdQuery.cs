using MediatR;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetById
{
    public record GetSubDetailsByIdQuery(int Id) : IRequest<Result<SubscriptionDetailsDto>>;
}
