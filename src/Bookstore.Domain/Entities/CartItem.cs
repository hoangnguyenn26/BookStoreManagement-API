
namespace Bookstore.Domain.Entities
{
    public class CartItem
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Book Book { get; set; } = null!;
    }
}