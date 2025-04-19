
using Bookstore.Domain.Entities;
namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IStockReceiptRepository : IGenericRepository<StockReceipt>
    {
        Task<StockReceipt?> GetReceiptWithDetailsByIdAsync(Guid id, bool tracking = false, CancellationToken cancellationToken = default);
        Task<IEnumerable<StockReceipt>> GetAllReceiptsAsync(int page, int pageSize, bool tracking = false, CancellationToken cancellationToken = default);
    }
}