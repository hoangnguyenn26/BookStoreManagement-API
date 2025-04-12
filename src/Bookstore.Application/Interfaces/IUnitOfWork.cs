// src/Bookstore.Application/Interfaces/IUnitOfWork.cs
using Bookstore.Domain.Interfaces.Repositories;

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
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}