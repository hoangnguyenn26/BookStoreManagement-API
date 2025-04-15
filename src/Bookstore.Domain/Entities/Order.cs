using Bookstore.Domain.Enums; // Namespace chứa OrderStatus

namespace Bookstore.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }

        // Shipping Address Snapshot
        public Guid? OrderShippingAddressId { get; set; }
        public OrderType OrderType { get; set; } // Sử dụng Enum OrderType
        public PaymentMethod? PaymentMethod { get; set; } // Sử dụng Enum PaymentMethod (Nullable)
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending; // Sử dụng Enum PaymentStatus
        public DeliveryMethod DeliveryMethod { get; set; } // Sử dụng Enum DeliveryMethod
        public string? TransactionId { get; set; } // Mã giao dịch online
        public string? InvoiceNumber { get; set; } // Số hóa đơn

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual OrderShippingAddress? OrderShippingAddress { get; set; } // Navigation property (NULLABLE)
    }
}