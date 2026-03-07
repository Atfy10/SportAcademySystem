using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    public interface ITraineeCodeGenerator
    {
        Task<string> GenerateAsync(
            int traineeId,
            int branchId,
            int nationalityCategoryId,
            AgeCategory ageCategory,
            int familyId,
            CancellationToken cancellationToken);
    }
}
