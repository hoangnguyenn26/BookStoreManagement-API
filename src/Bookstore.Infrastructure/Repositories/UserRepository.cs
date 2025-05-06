
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions; // Cần cho FirstOrDefaultAsync

namespace Bookstore.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Implement các phương thức đặc thù của IUserRepository
        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                 .Include(u => u.UserRoles)
                     .ThenInclude(ur => ur.Role)
                     .AsNoTracking()
                 .FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByIdWithRolesAsync(Guid userId, bool tracking = false, CancellationToken cancellationToken = default)
        {
            IQueryable<User> query = _dbSet
                .Where(u => u.Id == userId)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role);

            if (!tracking) query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetAllWithRolesAsync(Expression<Func<User, bool>>? filter = null, int page = 1, int pageSize = 10, bool tracking = false, CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            IQueryable<User> query = _dbSet
                 .Include(u => u.UserRoles)
                     .ThenInclude(ur => ur.Role);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            query = query.OrderBy(u => u.UserName);

            if (!tracking) query = query.AsNoTracking();

            return await query.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(cancellationToken);
        }
    }
}