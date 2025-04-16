// src/Bookstore.Domain/Interfaces/Repositories/IOrderRepository.cs
using Bookstore.Domain.Entities;
using System.Linq.Expressions;

namespace Bookstore.Domain.Interfaces.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        // Lấy tất cả Orders cho Admin (có thể thêm phân trang, lọc, sắp xếp)
        Task<IEnumerable<Order>> GetAllOrdersAsync(Expression<Func<Order, bool>>? filter = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    }
}