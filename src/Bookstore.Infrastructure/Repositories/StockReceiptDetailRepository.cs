
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
namespace Bookstore.Infrastructure.Repositories
{
    public class StockReceiptDetailRepository : IStockReceiptDetailRepository
    {
        private readonly ApplicationDbContext _context;
        public StockReceiptDetailRepository(ApplicationDbContext context) { _context = context; }

        public async Task<StockReceiptDetail> AddAsync(StockReceiptDetail detail, CancellationToken cancellationToken = default)
        {
            await _context.StockReceiptDetails.AddAsync(detail, cancellationToken);
            return detail;
        }
        public async Task AddRangeAsync(IEnumerable<StockReceiptDetail> details, CancellationToken cancellationToken = default)
        {
            await _context.StockReceiptDetails.AddRangeAsync(details, cancellationToken);
        }
    }
}