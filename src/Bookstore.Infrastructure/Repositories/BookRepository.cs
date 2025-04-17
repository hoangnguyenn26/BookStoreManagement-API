
using Bookstore.Domain.Entities;
using Bookstore.Domain.Enums;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Domain.Queries;
using Bookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure.Repositories
{
    public class BookRepository : GenericRepository<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<BestsellerInfo>> GetBestsellersInfoAsync(DateTime startDate, DateTime endDate, int top, CancellationToken cancellationToken = default)
        {
            var inclusiveEndDate = endDate.Date.AddDays(1);
            var inclusiveStartDate = startDate.Date;

            var bestsellersQuery = _context.OrderDetails
                .Where(od => od.Order.Status == OrderStatus.Completed &&
                             od.Order.OrderDate >= inclusiveStartDate &&
                             od.Order.OrderDate < inclusiveEndDate)
                .GroupBy(od => od.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    TotalQuantitySold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(top)
                .Join(_context.Books,
                      bestsellerInfo => bestsellerInfo.BookId,
                      book => book.Id,
                      (bestsellerInfo, book) => new BestsellerInfo
                      {
                          BookId = bestsellerInfo.BookId,
                          BookTitle = book.Title,
                          TotalQuantitySold = bestsellerInfo.TotalQuantitySold
                      });

            // Thực thi truy vấn và trả về kết quả
            return await bestsellersQuery.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}