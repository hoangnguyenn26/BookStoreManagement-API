
namespace Bookstore.Domain.Entities
{
    public class WishlistItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow; 

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Book Book { get; set; } = null!;
    }
}