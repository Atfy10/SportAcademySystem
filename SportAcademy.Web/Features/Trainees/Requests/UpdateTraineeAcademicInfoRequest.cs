namespace SportAcademy.Web.Features.Trainees.Requests;

public record UpdateTraineeAcademicInfoRequest(
    int BranchId,
    List<int> SportIds
);
