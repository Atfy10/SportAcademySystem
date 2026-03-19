using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAllForDropdown;

public record GetAllTraineeGroupsForDropdownQuery : IRequest<Result<List<TraineeGroupDropdownDto>>>;
