
using Bookstore.Domain.Entities;
using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bookstore.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync(Expression<Func<Order, bool>>? filter = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            IQueryable<Order> query = _dbSet.Include(o => o.User)
                                            .Include(o => o.OrderShippingAddress);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.OrderByDescending(o => o.OrderDate)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .AsNoTracking()
                              .ToListAsync(cancellationToken);
        }

        public async Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            // Include các thông tin cần thiết khi xem chi tiết đơn hàng
            return await _dbSet
                .Include(o => o.User)
                .Include(o => o.OrderShippingAddress)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderShippingAddress)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}