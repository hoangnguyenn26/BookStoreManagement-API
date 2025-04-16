
using Bookstore.Application.Dtos.Addresses;
using Bookstore.Domain.Enums;

namespace Bookstore.Application.Dtos.Orders
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderType OrderType { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        // Địa chỉ giao hàng (từ bảng snapshot)
        public AddressDto? ShippingAddress { get; set; }

        // Chi tiết đơn hàng
        public List<OrderDetailDto> OrderDetails { get; set; } = new List<OrderDetailDto>();
    }
}