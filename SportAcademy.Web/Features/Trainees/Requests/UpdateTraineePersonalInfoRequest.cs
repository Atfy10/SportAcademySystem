namespace SportAcademy.Web.Features.Trainees.Requests;

public record UpdateTraineePersonalInfoRequest(
    string? FirstName,
    string? LastName,
    string? GuardianName,
    string? ParentNumber,
    List<string>? MedicalConditions,
    string? ImageUrl
);
