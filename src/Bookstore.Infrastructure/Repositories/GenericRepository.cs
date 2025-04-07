using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence; // Namespace DbContext
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bookstore.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeProperties = null,
            bool isTracking = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            // Apply filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Apply includes
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            // Apply orderBy
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Apply tracking
            if (!isTracking)
            {
                query = query.AsNoTracking(); // Quan trọng: Tắt tracking cho các truy vấn chỉ đọc
            }

            return await query.ToListAsync(cancellationToken);
        }


        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            // ID và CreatedAtUtc/UpdatedAtUtc được xử lý bởi BaseEntity và SaveChangesAsync override
            return entity;
        }

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            // Chỉ cần đánh dấu trạng thái là Modified, SaveChangesAsync sẽ xử lý
            _context.Entry(entity).State = EntityState.Modified;
            // Cập nhật UpdatedAtUtc sẽ được xử lý trong SaveChangesAsync override
            return Task.CompletedTask; // Không cần thao tác I/O bất đồng bộ ở đây
        }

        public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                await DeleteAsync(entity, cancellationToken);
            }
            // Có thể throw exception nếu không tìm thấy entity
        }

        public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            // Kiểm tra xem entity có hỗ trợ Soft Delete không
            if (entity is ISoftDeleteEntity softDeleteEntity) // Giả sử có interface ISoftDeleteEntity { bool IsDeleted { get; set; } }
            {
                softDeleteEntity.IsDeleted = true;
                _context.Entry(entity).State = EntityState.Modified; // Đánh dấu là Modified để SaveChanges lưu cờ IsDeleted
            }
            else
            {
                // Nếu không hỗ trợ soft delete, xóa vật lý
                _dbSet.Remove(entity);
            }
            return Task.CompletedTask; // Không cần thao tác I/O bất đồng bộ ở đây
        }

        // (Optional) Interface cho Soft Delete
        // namespace Bookstore.Domain.Interfaces { public interface ISoftDeleteEntity { bool IsDeleted { get; set; } } }
        // Các Entity Book, Category cần implement ISoftDeleteEntity
    }
}