
using Bookstore.Application.Dtos.Orders;
using Bookstore.Domain.Enums;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOnlineOrderAsync(Guid userId, CreateOrderRequestDto createOrderDto, CancellationToken cancellationToken = default);

        Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<OrderDto?> GetOrderByIdForUserAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default);

        Task<IEnumerable<OrderSummaryDto>> GetAllOrdersForAdminAsync(int page = 1, int pageSize = 10, string? statusFilter = null, CancellationToken cancellationToken = default);

        Task<OrderDto?> GetOrderByIdForAdminAsync(Guid orderId, CancellationToken cancellationToken = default);

        Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, Guid adminUserId, CancellationToken cancellationToken = default);

        Task<bool> CancelOrderAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default);


    }
}