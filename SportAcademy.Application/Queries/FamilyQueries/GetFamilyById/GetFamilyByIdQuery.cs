using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FamilyDtos;

namespace SportAcademy.Application.Queries.FamilyQueries.GetFamilyById;

public record GetFamilyByIdQuery(int Id) : IRequest<Result<FamilyDetailDto>>;
