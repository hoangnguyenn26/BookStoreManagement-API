// src/Bookstore.Domain/Interfaces/Repositories/IGenericRepository.cs
using Bookstore.Domain.Entities; // Namespace chứa BaseEntity
using System.Linq.Expressions; // Cần cho Expression

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity // Ràng buộc T phải là lớp kế thừa BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default); // Đơn giản nhất

        // Phiên bản GetAllAsync linh hoạt hơn (tùy chọn, có thể thêm sau nếu cần)
        Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? filter = null,
                                      Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                      string? includeProperties = null, // Ví dụ: "Category,Author"
                                      bool isTracking = false, // Mặc định không theo dõi thay đổi cho truy vấn đọc
                                      CancellationToken cancellationToken = default);

        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        // Update và Delete thường không cần async trong cách dùng EF Core phổ biến
        // vì chúng chỉ thay đổi trạng thái của entity trong context.
        // Nhưng để interface nhất quán, ta có thể vẫn để Task (dù Task.CompletedTask)
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default); // Hoặc void Update(T entity);

        Task DeleteAsync(T entity, CancellationToken cancellationToken = default); // Hoặc void Delete(T entity);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default); // Xóa theo Id
    }
}