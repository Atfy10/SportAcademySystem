using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetOwnerById;

public record GetOwnerByIdQuery(Guid OwnerId) : IRequest<Result<OwnerDetailDto>>;
