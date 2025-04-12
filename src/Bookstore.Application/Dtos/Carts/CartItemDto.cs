
using Bookstore.Application.Dtos.Books;

namespace Bookstore.Application.Dtos.Carts
{
    public class CartItemDto
    {
        public Guid BookId { get; set; }
        public int Quantity { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        // Nhúng thông tin sách
        public BookDto Book { get; set; } = null!;
    }
}