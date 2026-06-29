using SportAcademy.Domain.Entities;

namespace SportAcademy.Domain.Contract;

public interface IInvitationRepository
{
    Task<Invitation?> FindByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(Invitation invitation, CancellationToken ct = default);
}
