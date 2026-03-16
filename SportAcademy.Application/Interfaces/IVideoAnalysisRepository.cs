using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces;

public interface IVideoAnalysisRepository : IBaseRepository<VideoAnalysis, Guid>
{
    Task<List<VideoAnalysis>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
}
