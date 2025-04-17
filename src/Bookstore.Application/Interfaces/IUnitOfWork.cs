using Bookstore.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
namespace Bookstore.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository CategoryRepository { get; }
        IUserRepository UserRepository { get; }
        IBookRepository BookRepository { get; }
        IAuthorRepository AuthorRepository { get; }
        IInventoryLogRepository InventoryLogRepository { get; }
        IRoleRepository RoleRepository { get; }
        IWishlistRepository WishlistRepository { get; }
        ICartRepository CartRepository { get; }
        IAddressRepository AddressRepository { get; }
        IOrderRepository OrderRepository { get; }
        IOrderShippingAddressRepository OrderShippingAddressRepository { get; }
        IPromotionRepository PromotionRepository { get; }
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}