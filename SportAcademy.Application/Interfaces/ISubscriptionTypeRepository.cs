using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
	public interface ISubscriptionTypeRepository : IBaseRepository<SubscriptionType, int>
	{
		Task<List<SubscriptionType>> GetAllWithSportsAsync(CancellationToken cancellationToken = default);
		Task<SubscriptionType?> GetByIdWithSportsAsync(int id, CancellationToken cancellationToken = default);
	}
}