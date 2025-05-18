using Bookstore.Domain.Entities;
using System.Linq.Expressions;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IInventoryLogRepository
    {
        Task<InventoryLog> AddAsync(InventoryLog entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryLog>> GetHistoryByBookIdAsync(Guid bookId, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<InventoryLog> entities, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryLog>> GetHistoryAsync(
            Expression<Func<InventoryLog, bool>>? filter = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<InventoryLog, bool>>? filter = null, CancellationToken cancellationToken = default);
    }
}