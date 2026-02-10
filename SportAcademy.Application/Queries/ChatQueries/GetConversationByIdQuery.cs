using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace SportAcademy.Application.Queries.ChatQueries
{
    public record GetConversationByIdQuery(
    Guid Id
) : IRequest<Result<ChatConversationDto>>;
}
