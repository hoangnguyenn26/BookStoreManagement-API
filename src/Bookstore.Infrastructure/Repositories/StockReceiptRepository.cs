
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure.Repositories
{
    public class StockReceiptRepository : GenericRepository<StockReceipt>, IStockReceiptRepository
    {
        public StockReceiptRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<StockReceipt>> GetAllReceiptsAsync(int page = 1, int pageSize = 10, bool tracking = false, CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            IQueryable<StockReceipt> query = _dbSet.Include(sr => sr.Supplier);

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            var results = await query.OrderByDescending(sr => sr.ReceiptDate)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(cancellationToken);

            return results;
        }

        public async Task<StockReceipt?> GetReceiptWithDetailsByIdAsync(Guid id, bool tracking = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(sr => sr.Supplier)
                .Include(sr => sr.StockReceiptDetails)
                    .ThenInclude(srd => srd.Book)
                .Where(sr => sr.Id == id);

            if (!tracking) query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
    }
}