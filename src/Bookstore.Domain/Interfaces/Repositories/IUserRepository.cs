
using Bookstore.Domain.Entities;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        // Có thể thêm các phương thức truy vấn đặc thù cho User sau này
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetByIdWithRolesAsync(Guid userId, bool tracking = false, CancellationToken cancellationToken = default);

        Task<IEnumerable<User>> GetAllWithRolesAsync(int page = 1, int pageSize = 10, bool tracking = false, CancellationToken cancellationToken = default);
    }
}