﻿
using Bookstore.Domain.Entities;
using System.Linq.Expressions; 

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity 
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default); // Đơn giản nhất
        Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>>? filter = null, // Điều kiện lọc (WHERE)
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, // Sắp xếp (ORDER BY)
            string? includeProperties = null, // Các navigation property cần include
            bool isTracking = false, // Có theo dõi thay đổi không
            CancellationToken cancellationToken = default);

        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);


        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        Task DeleteAsync(T entity, CancellationToken cancellationToken = default); 

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default); // Xóa theo Id
    }
}