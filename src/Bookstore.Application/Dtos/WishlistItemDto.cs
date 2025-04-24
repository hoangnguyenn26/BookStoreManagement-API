
using Bookstore.Application.Dtos.Books;

namespace Bookstore.Application.Dtos.Wishlists
{
    public class WishlistItemDto
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public BookDto Book { get; set; } = null!;
    }
}