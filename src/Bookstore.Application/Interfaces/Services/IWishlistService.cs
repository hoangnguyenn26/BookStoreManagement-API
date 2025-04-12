
using Bookstore.Application.Dtos.Wishlists;


namespace Bookstore.Application.Interfaces.Services
{
    public interface IWishlistService
    {
        Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(Guid userId, CancellationToken cancellationToken = default);
        // Trả về bool cho biết thao tác thành công hay sách đã tồn tại/lỗi
        Task<bool> AddToWishlistAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);
        Task<bool> RemoveFromWishlistAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);
        Task<bool> RemoveFromWishlistByIdAsync(Guid userId, Guid wishlistItemId, CancellationToken cancellationToken = default);
    }
}