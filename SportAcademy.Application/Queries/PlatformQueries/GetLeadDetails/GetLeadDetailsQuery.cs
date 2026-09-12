using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetLeadDetails;

public record GetLeadDetailsQuery(Guid LeadId) : IRequest<Result<LeadDetailDto>>;
