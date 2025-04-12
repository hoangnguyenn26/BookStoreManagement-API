
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Dtos.Carts
{
    public class AddCartItemDto
    {
        [Required]
        public Guid BookId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;
    }
}