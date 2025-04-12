
using Bookstore.Domain.Entities;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface ICartRepository
    {
        Task<CartItem?> GetByUserIdAndBookIdAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CartItem>> GetCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<CartItem> AddAsync(CartItem cartItem, CancellationToken cancellationToken = default);
        Task UpdateAsync(CartItem cartItem, CancellationToken cancellationToken = default);
        Task DeleteAsync(CartItem cartItem, CancellationToken cancellationToken = default);
        Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}