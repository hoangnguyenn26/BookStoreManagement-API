
using Bookstore.Application.Dtos.Carts;

namespace Bookstore.Application.Interfaces.Services
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemDto>> GetUserCartAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<CartItemDto?> AddOrUpdateCartItemAsync(Guid userId, AddCartItemDto addItemDto, CancellationToken cancellationToken = default);

        Task<bool> UpdateCartItemQuantityAsync(Guid userId, Guid bookId, UpdateCartItemDto updateDto, CancellationToken cancellationToken = default);

        Task<bool> RemoveCartItemAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);

        Task<bool> ClearUserCartAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}