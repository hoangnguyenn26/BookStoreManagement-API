
using Bookstore.Domain.Entities;
namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IStockReceiptDetailRepository
    {
        Task<StockReceiptDetail> AddAsync(StockReceiptDetail detail, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<StockReceiptDetail> details, CancellationToken cancellationToken = default);
    }
}