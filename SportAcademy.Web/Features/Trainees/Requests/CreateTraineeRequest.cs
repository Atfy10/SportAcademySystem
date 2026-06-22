using MediatR;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Features.Trainees.Requests;

public record CreateTraineeRequest(
    string FirstName,
    string LastName,
    string SSN,
    int FamilyId,
    int NationalityCategoryId,
    string? ParentNumber,
    string? GuardianName,
    DateOnly BirthDate,
    Gender Gender,
    string? AppUserId,
    int BranchId,
    HashSet<int> SportIds,
    // Person base class fields:
    string PhoneNumber,
    string Email,
    Nationality Nationality,
    string? Street,
    string? City
);