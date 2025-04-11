
using Bookstore.Domain.Entities;


namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);
        Task<IList<string>?> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}