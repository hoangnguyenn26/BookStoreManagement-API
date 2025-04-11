
using Bookstore.Domain.Enums;

namespace Bookstore.Domain.Entities
{
    public class InventoryLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BookId { get; set; }
        public int ChangeQuantity { get; set; }
        public InventoryReason Reason { get; set; } 
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public Guid? OrderId { get; set; } 
        public Guid? UserId { get; set; }
        public virtual Book Book { get; set; } = null!;
        public virtual Order? Order { get; set; }
        public virtual User? User { get; set; }
    }
}