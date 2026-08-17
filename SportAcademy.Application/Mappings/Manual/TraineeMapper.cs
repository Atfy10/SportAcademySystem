using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Application.Mappings.Manual
{
    // Hand-written replacement for the AutoMapper CreateTraineeCommand/UpdateTraineePersonalCommand
    // <-> Trainee mappings in TraineeProfile.cs. Only covers the command-to-entity direction used
    // by CreateTraineeCommandHandler/UpdateTraineePersonalCommandHandler; the DTO-facing mappings
    // in TraineeProfile stay on AutoMapper until their own handlers are touched.
    public static class TraineeMapper
    {
        public static Trainee ToEntity(CreateTraineeCommand cmd)
        {
            var street = cmd.Street ?? "";
            var city = cmd.City ?? "";
            if (string.IsNullOrWhiteSpace(street) && string.IsNullOrWhiteSpace(city))
                street = city = "-";

            return new Trainee
            {
                FirstName = cmd.FirstName,
                LastName = cmd.LastName,
                SSN = cmd.SSN,
                Email = Email.Create(cmd.Email),
                BirthDate = cmd.BirthDate,
                Gender = cmd.Gender,
                Nationality = cmd.Nationality,
                Address = Address.Create(street, city),
                PhoneNumber = cmd.PhoneNumber,
                AppUserId = cmd.AppUserId,
                BranchId = cmd.BranchId,
                NationalityCategoryId = cmd.NationalityCategoryId,
                ParentNumber = cmd.ParentNumber,
                GuardianName = cmd.GuardianName,
                Sports = cmd.SportIds.Select(id => new SportTrainee { SportId = id }).ToList(),
            };
        }

        // Mirrors AutoMapper's Map(request, trainee) for UpdateTraineePersonalCommand -> Trainee:
        // only overwrites fields the command actually carries, and — same as the AutoMapper
        // config it replaces — leaves Sports untouched (handled separately via UpdateSports).
        public static void ApplyPersonalUpdate(Trainee trainee, UpdateTraineePersonalCommand cmd)
        {
            if (cmd.FirstName != null) trainee.FirstName = cmd.FirstName;
            if (cmd.LastName != null) trainee.LastName = cmd.LastName;
            if (cmd.GuardianName != null) trainee.GuardianName = cmd.GuardianName;
            if (cmd.ParentNumber != null) trainee.ParentNumber = cmd.ParentNumber;
            trainee.BranchId = cmd.BranchId;
        }
    }
}
