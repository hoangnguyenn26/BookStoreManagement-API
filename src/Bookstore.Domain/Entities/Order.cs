using Bookstore.Domain.Enums; // Namespace chứa OrderStatus
using System.ComponentModel.DataAnnotations; // Cần cho Timestamp

namespace Bookstore.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // Thường là lúc tạo
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }

        // Shipping Address Snapshot (Lưu trực tiếp vào Order)
        public string ShippingAddress_Street { get; set; } = null!;
        public string ShippingAddress_City { get; set; } = null!;
        public string? ShippingAddress_State { get; set; }
        public string ShippingAddress_PostalCode { get; set; } = null!;
        public string ShippingAddress_Country { get; set; } = null!;
        public string? ShippingAddress_RecipientName { get; set; }
        public string? ShippingAddress_PhoneNumber { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}