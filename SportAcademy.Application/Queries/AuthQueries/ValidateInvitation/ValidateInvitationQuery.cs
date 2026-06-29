using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;

namespace SportAcademy.Application.Queries.AuthQueries.ValidateInvitation;

public record ValidateInvitationQuery(string RawToken) : IRequest<Result<InvitationResponse>>;
