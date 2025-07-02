using Bookstore.Domain.Entities;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IReviewRepository : IGenericRepository<Reviews>
    {
        Task<IEnumerable<Reviews>> GetReviewsByBookIdAsync(Guid bookId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<Reviews?> GetByUserIdAndBookIdAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);
    }
}