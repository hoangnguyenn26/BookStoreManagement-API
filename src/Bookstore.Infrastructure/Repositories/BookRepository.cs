
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

            return await bestsellersQuery.AsNoTracking().ToListAsync(cancellationToken);
        }
        public async Task<Book?> GetBookWithDetailsByIdAsync(Guid id, bool tracking = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Book> query = _dbSet
                  .Include(b => b.Author)
                  .Include(b => b.Category);
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }
        public async Task<IEnumerable<Book>> GetBooksFilteredAsync(
            Guid? categoryId = null,
            string? search = null,
            int page = 1,
            int pageSize = 10,
            string? sortBy = null,
            string? sortDirection = "asc",
            CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            IQueryable<Book> query = _dbSet
                                        .Include(b => b.Author)
                                        .Include(b => b.Category);

            if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            // Lọc theo Search Term
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim().ToLower();
                query = query.Where(b =>
                    (b.Title != null && EF.Functions.Like(b.Title.ToLower(), $"%{searchTerm}%")) ||
                    (b.Author != null && b.Author.Name != null && EF.Functions.Like(b.Author.Name.ToLower(), $"%{searchTerm}%")) ||
                    (b.ISBN != null && EF.Functions.Like(b.ISBN.ToLower(), $"%{searchTerm}%"))
                );
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                bool isAscending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

                switch (sortBy.ToLowerInvariant())
                {
                    case "title":
                        query = isAscending ? query.OrderBy(b => b.Title) : query.OrderByDescending(b => b.Title);
                        break;
                    case "price":
                        query = isAscending ? query.OrderBy(b => b.Price) : query.OrderByDescending(b => b.Price);
                        break;
                    case "date":
                    default:
                        query = isAscending ? query.OrderBy(b => b.CreatedAtUtc) : query.OrderByDescending(b => b.CreatedAtUtc);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(b => b.CreatedAtUtc);
            }
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return await query.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}