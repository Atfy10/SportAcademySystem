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
    Guid? AppUserId,
    int BranchId,
    HashSet<int> SportIds,
    // Person base class fields:
    string PhoneNumber,
    string Email,
    Nationality Nationality,
    string? Street,
    string? City,
    // Sent by the create form all along but previously missing here, so both were silently
    // dropped on every new trainee.
    List<string>? MedicalConditions = null,
    string? ImageUrl = null
);