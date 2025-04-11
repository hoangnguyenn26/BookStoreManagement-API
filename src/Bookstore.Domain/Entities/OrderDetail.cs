
namespace Bookstore.Domain.Entities
{
    // Không cần BaseEntity nếu Id không cần các trường Audit riêng
    public class OrderDetail
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 
        public Guid OrderId { get; set; }
        public Guid BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } 

        // Navigation properties
        public virtual Order Order { get; set; } = null!;
        public virtual Book Book { get; set; } = null!;
    }
}