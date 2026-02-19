using SportAcademy.Application.DTOs.ChatDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.ChatQueries
{
    public record GetConversationByIdQuery(
    Guid Id
) : IRequest<Result<ChatConversationDto>>;
}
