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
        public virtual async Task<IReadOnlyList<T>> ListAsync(
           Expression<Func<T, bool>>? filter = null,
           Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
           string? includeProperties = null, 
           bool isTracking = false,
           CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            // 1. Áp dụng Tracking (Mặc định là NoTracking cho hiệu năng đọc)
            if (!isTracking)
            {
                query = query.AsNoTracking();
            }

            // 2. Áp dụng Filter (WHERE clause)
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // 3. Áp dụng Includes (Eager Loading)
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            // 4. Áp dụng OrderBy (ORDER BY clause)
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // 5. Thực thi truy vấn và trả về kết quả List
            return await query.ToListAsync(cancellationToken);
        }
        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
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
            if (entity is ISoftDeleteEntity softDeleteEntity)
            {
                softDeleteEntity.IsDeleted = true;
                _context.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                _dbSet.Remove(entity);
            }
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}