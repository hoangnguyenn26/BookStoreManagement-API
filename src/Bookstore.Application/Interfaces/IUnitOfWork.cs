// src/Bookstore.Application/Interfaces/IUnitOfWork.cs
using Bookstore.Domain.Interfaces.Repositories; // Namespace chứa các repo interfaces
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bookstore.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository CategoryRepository { get; }
        IUserRepository UserRepository { get; }
        IBookRepository BookRepository { get; }
        IAuthorRepository AuthorRepository { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}