using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAllForDropdown;

// SkillLevel is the trainee's own recorded level for SportId - the query returns only groups
// whose required SkillLevel is at or below it (a trainee can always join an easier group).
// Gender is the trainee's own gender - the query returns only groups whose gender policy
// accepts them (Mixed groups always match). Both null = unfiltered on that axis, same as
// SportId - used for the "no trainee/subscription picked yet" case.
public record GetAllTraineeGroupsForDropdownQuery(
    int? SportId = null,
    SkillLevel? SkillLevel = null,
    Gender? Gender = null
) : IRequest<Result<List<TraineeGroupDropdownDto>>>;
