using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Contract;

public interface IInvitationRepository
{
    Task<Invitation?> FindByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<List<Invitation>> GetPendingByTenantAndEmailAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task AddAsync(Invitation invitation, CancellationToken ct = default);
}
