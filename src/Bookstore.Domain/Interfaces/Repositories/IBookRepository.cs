
using Bookstore.Domain.Entities;
using Bookstore.Domain.Queries;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        Task<IEnumerable<BestsellerInfo>> GetBestsellersInfoAsync(DateTime startDate, DateTime endDate, int top, CancellationToken cancellationToken = default);
    }
}