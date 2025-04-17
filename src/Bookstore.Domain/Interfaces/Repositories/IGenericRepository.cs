
using Bookstore.Domain.Entities;
using System.Linq.Expressions;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeProperties = null,
            bool isTracking = false,
            // ----- Thêm tham số phân trang -----
            int? page = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default);

        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);


        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default); // Xóa theo Id
        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default);
        Task<decimal> SumAsync(
            Expression<Func<T, decimal>> selector,     // <<-- Đưa tham số bắt buộc lên trước
            Expression<Func<T, bool>>? filter = null, // <<-- Tham số tùy chọn ra sau
            CancellationToken cancellationToken = default
        );

    }
}