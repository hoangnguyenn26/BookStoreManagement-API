// src/Bookstore.Domain/Interfaces/Repositories/IGenericRepository.cs
using Bookstore.Domain.Entities; // Namespace chứa BaseEntity
using System.Linq.Expressions; // Cần cho Expression

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity // Ràng buộc T phải là lớp kế thừa BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default); // Đơn giản nhất


        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);


        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        Task DeleteAsync(T entity, CancellationToken cancellationToken = default); 

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default); // Xóa theo Id
    }
}