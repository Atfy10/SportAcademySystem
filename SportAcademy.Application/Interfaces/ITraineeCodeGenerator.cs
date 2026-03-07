using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces
{
    public interface ITraineeCodeGenerator
    {
        Task<string> GenerateAsync(
            int familyId,
            int branchId,
            int nationalityCategoryId,
            AgeCategory ageCategory,
            CancellationToken cancellationToken);
    }
}
