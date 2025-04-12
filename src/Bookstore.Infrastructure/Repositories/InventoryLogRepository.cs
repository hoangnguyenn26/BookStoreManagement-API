using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore; // Cần cho ToListAsync, OrderByDescending
using System.Collections.Generic;
using System.Linq; // Cần cho Where, OrderByDescending
using System.Threading;
using System.Threading.Tasks;

namespace Bookstore.Infrastructure.Repositories
{
    public class InventoryLogRepository : IInventoryLogRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryLogRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
    }
}