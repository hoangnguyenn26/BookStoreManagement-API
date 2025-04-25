
using Bookstore.Domain.Entities;
using Bookstore.Domain.Queries;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        Task<Book?> GetBookWithDetailsByIdAsync(Guid id, bool tracking = false, CancellationToken cancellationToken = default);
        Task<IEnumerable<Book>> GetBooksFilteredAsync(
            Guid? categoryId = null,
            string? search = null,
            int page = 1,
            int pageSize = 10,
            string? sortBy = null,
            string? sortDirection = "asc",
            CancellationToken cancellationToken = default);
        Task<IEnumerable<BestsellerInfo>> GetBestsellersInfoAsync(DateTime startDate, DateTime endDate, int top, CancellationToken cancellationToken = default);
    }
}