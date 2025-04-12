
using Bookstore.Domain.Entities;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IWishlistRepository
    {
        Task<WishlistItem?> GetByIdAsync(Guid wishlistItemId, CancellationToken cancellationToken = default); 
        Task<WishlistItem?> GetByUserIdAndBookIdAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WishlistItem>> GetWishlistByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<WishlistItem> AddAsync(WishlistItem wishlistItem, CancellationToken cancellationToken = default);
        //Task DeleteAsync(WishlistItem wishlistItem, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid wishlistItemId, CancellationToken cancellationToken = default);
    }
}