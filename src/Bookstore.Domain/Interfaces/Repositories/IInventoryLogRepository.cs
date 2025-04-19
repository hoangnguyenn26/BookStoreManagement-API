using Bookstore.Domain.Entities;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IInventoryLogRepository
    {
        Task<InventoryLog> AddAsync(InventoryLog entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<InventoryLog>> GetHistoryByBookIdAsync(Guid bookId, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<InventoryLog> entities, CancellationToken cancellationToken = default);
    }
}