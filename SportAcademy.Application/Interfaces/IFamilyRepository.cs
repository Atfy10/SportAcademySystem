using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface IFamilyRepository : IBaseRepository<Family, int>
    {
        Task<IReadOnlyList<TFamilyDto>> SearchFamiliesWithCode<TFamilyDto>(int code, CancellationToken cancellationToken = default)
            where TFamilyDto : class;
    }
}
