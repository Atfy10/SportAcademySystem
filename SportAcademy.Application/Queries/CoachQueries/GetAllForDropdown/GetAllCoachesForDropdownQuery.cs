using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.CoachDtos;

namespace SportAcademy.Application.Queries.CoachQueries.GetAllForDropdown;

public record GetAllCoachesForDropdownQuery : IRequest<Result<List<CoachDropdownItemDto>>>;
