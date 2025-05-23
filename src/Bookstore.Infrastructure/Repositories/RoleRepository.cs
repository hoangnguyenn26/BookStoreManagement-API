﻿using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower(), cancellationToken);
        }

        public async Task<IList<string>?> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var roles = await _context.UserRoles
                                  .Where(ur => ur.UserId == userId)
                                  .Include(ur => ur.Role)
                                  .Select(ur => ur.Role.Name)
                                  .ToListAsync(cancellationToken);
            return roles;
        }
    }
}