using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions; // Cần cho ToListAsync, OrderByDescending

namespace Bookstore.Infrastructure.Repositories
{
    public class InventoryLogRepository : IInventoryLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<InventoryLog> _dbSet;

        public InventoryLogRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<InventoryLog>();
        }

        public async Task<InventoryLog> AddAsync(InventoryLog entity, CancellationToken cancellationToken = default)
        {
            await _context.InventoryLogs.AddAsync(entity, cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<InventoryLog>> GetHistoryByBookIdAsync(Guid bookId, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryLogs
                                 .Where(log => log.BookId == bookId)
                                 .OrderByDescending(log => log.TimestampUtc)
                                 .AsNoTracking()
                                 .ToListAsync(cancellationToken);
        }
        public async Task AddRangeAsync(IEnumerable<InventoryLog> entities, CancellationToken cancellationToken = default)
        {
            await _context.InventoryLogs.AddRangeAsync(entities, cancellationToken);
        }
        public async Task<IEnumerable<InventoryLog>> GetHistoryAsync(
           Expression<Func<InventoryLog, bool>>? filter = null,
           int page = 1,
           int pageSize = 20,
           CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            IQueryable<InventoryLog> query = _dbSet
                .Include(log => log.Book)
                .Include(log => log.User);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query
                .OrderByDescending(log => log.TimestampUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<InventoryLog, bool>>? filter = null, CancellationToken cancellationToken = default)
        {
            IQueryable<InventoryLog> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.CountAsync(cancellationToken);
        }
    }
}