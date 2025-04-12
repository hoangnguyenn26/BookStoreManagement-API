
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<CartItem> _dbSet;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<CartItem>();
        }

        public async Task<CartItem?> GetByUserIdAndBookIdAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { userId, bookId }, cancellationToken);
        }

        public async Task<IEnumerable<CartItem>> GetCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Book)
                    .ThenInclude(b => b.Author)
                .OrderBy(ci => ci.Book.Title)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<CartItem> AddAsync(CartItem cartItem, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(cartItem, cancellationToken);
            return cartItem;
        }

        public Task UpdateAsync(CartItem cartItem, CancellationToken cancellationToken = default)
        {
            _context.Entry(cartItem).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(CartItem cartItem, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(cartItem);
            return Task.CompletedTask;
        }

        public async Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var itemsToRemove = await _dbSet.Where(ci => ci.UserId == userId).ToListAsync(cancellationToken);
            if (itemsToRemove.Any())
            {
                _dbSet.RemoveRange(itemsToRemove);
            }
        }
    }
}