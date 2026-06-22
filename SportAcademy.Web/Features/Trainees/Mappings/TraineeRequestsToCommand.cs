using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Web.Features.Trainees.Requests;

namespace SportAcademy.Web.Features.Trainees.Mappings;

public static class TraineeRequestsToCommand
{
    public static CreateTraineeCommand ToCommand(this CreateTraineeRequest request)
        => new()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            SSN = request.SSN,
            FamilyId = request.FamilyId,
            NationalityCategoryId = request.NationalityCategoryId,
            ParentNumber = request.ParentNumber,
            GuardianName = request.GuardianName,
            BirthDate = request.BirthDate,
            Gender = request.Gender,
            AppUserId = request.AppUserId,
            BranchId = request.BranchId,
            SportIds = request.SportIds,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Nationality = request.Nationality,
            Street = request.Street,
            City = request.City,
        };

    public static UpdateTraineePersonalCommand ToCommand(this UpdateTraineeRequest request, int id)
        => new()
        {
            Id = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            GuardianName = request.GuardianName,
            ParentNumber = request.ParentNumber,
            BranchId = request.BranchId,
            SportIds = request.SportIds,
        };
}
