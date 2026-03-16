using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories;

public class VideoAnalysisRepository : BaseRepository<VideoAnalysis, Guid>, IVideoAnalysisRepository
{
    private readonly ApplicationDbContext _context;

    public VideoAnalysisRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<VideoAnalysis>> GetByUserIdAsync(
        string userId, CancellationToken cancellationToken)
    {
        return await _context.VideoAnalyses
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
