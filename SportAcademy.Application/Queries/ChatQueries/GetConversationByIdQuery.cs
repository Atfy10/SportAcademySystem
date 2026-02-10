using SportAcademy.Application.DTOs.ChatDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.DTOs.ChatDots;
using SportAcademy.Shared.Result;

namespace SportAcademy.Application.Queries.ChatQueries
{
    public record GetConversationByIdQuery(
    Guid Id
) : IRequest<Result<ChatConversationDto>>;
}
