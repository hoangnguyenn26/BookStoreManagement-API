
using Bookstore.Application.Dtos.Orders;
using Bookstore.Domain.Enums;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IOrderService
    {
        // Tạo đơn hàng mới (Online)
        Task<OrderDto> CreateOnlineOrderAsync(Guid userId, CreateOrderRequestDto createOrderDto, CancellationToken cancellationToken = default);

        // Lấy danh sách đơn hàng của User (phân trang)
        Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        // Lấy chi tiết đơn hàng của User (kiểm tra quyền sở hữu)
        Task<OrderDto?> GetOrderByIdForUserAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default);

        // Lấy tất cả đơn hàng cho Admin (phân trang, lọc)
        Task<IEnumerable<OrderSummaryDto>> GetAllOrdersForAdminAsync(int page = 1, int pageSize = 10, string? statusFilter = null, CancellationToken cancellationToken = default);

        // Lấy chi tiết đơn hàng cho Admin
        Task<OrderDto?> GetOrderByIdForAdminAsync(Guid orderId, CancellationToken cancellationToken = default);

        // Cập nhật trạng thái đơn hàng (Admin)
        Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, Guid adminUserId, CancellationToken cancellationToken = default);

        // Hủy đơn hàng (User)
        Task<bool> CancelOrderAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default);
    }
}