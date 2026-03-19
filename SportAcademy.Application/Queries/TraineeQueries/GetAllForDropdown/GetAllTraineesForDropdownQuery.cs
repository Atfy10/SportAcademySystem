using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;

namespace SportAcademy.Application.Queries.TraineeQueries.GetAllForDropdown;

public record GetAllTraineesForDropdownQuery : IRequest<Result<List<TraineeDropdownDto>>>;
