
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore; 


namespace Bookstore.Infrastructure.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<WishlistItem> _dbSet;

        public WishlistRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<WishlistItem>();
        }

        public async Task<WishlistItem?> GetByIdAsync(Guid wishlistItemId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { wishlistItemId }, cancellationToken);
        }

        public async Task<WishlistItem?> GetByUserIdAndBookIdAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(wi => wi.UserId == userId && wi.BookId == bookId, cancellationToken);
        }

        public async Task<IEnumerable<WishlistItem>> GetWishlistByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(wi => wi.UserId == userId)
                .Include(wi => wi.Book) 
                    .ThenInclude(b => b.Author) 
                .OrderByDescending(wi => wi.CreatedAtUtc) 
                .AsNoTracking() 
                .ToListAsync(cancellationToken);
        }

        public async Task<WishlistItem> AddAsync(WishlistItem wishlistItem, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(wishlistItem, cancellationToken);
            return wishlistItem;
        }

        public Task DeleteAsync(WishlistItem wishlistItem, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(wishlistItem);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid wishlistItemId, CancellationToken cancellationToken = default)
        {
            var item = await GetByIdAsync(wishlistItemId, cancellationToken);
            if (item != null)
            {
                await DeleteAsync(item, cancellationToken);
            }
        }
    }
}