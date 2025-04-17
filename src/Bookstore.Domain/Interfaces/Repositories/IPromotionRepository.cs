
using Bookstore.Domain.Entities;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IPromotionRepository : IGenericRepository<Promotion>
    {
        Task<Promotion?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        Task<IEnumerable<Promotion>> GetActivePromotionsAsync(DateTime currentDate, CancellationToken cancellationToken = default);
    }
}