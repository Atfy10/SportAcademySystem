namespace SportAcademy.Web.Features.Trainees.Requests;

public record UpdateTraineeRequest(
    string? FirstName,
    string? LastName,
    string? GuardianName,
    string? ParentNumber,
    int BranchId,
    List<int> SportIds
);
