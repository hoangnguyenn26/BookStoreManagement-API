using Bookstore.Domain.Entities;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByBookIdAsync(Guid bookId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<Review?> GetByUserIdAndBookIdAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);
    }
}