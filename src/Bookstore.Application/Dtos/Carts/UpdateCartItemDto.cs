
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Dtos.Carts
{
    public class UpdateCartItemDto
    {
        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}