using Bookstore.Domain.Enums; // Namespace chứa OrderStatus

namespace Bookstore.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid? UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }

        // Shipping Address Snapshot
        public Guid? OrderShippingAddressId { get; set; }
        public OrderType OrderType { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public DeliveryMethod DeliveryMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? InvoiceNumber { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual OrderShippingAddress? OrderShippingAddress { get; set; }
    }
}