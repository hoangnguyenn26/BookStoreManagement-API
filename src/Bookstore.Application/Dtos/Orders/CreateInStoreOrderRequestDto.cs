
using Bookstore.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Dtos.Orders
{
    public class InStoreOrderDetailDto
    {
        [Required]
        public Guid BookId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }
    }

    public class CreateInStoreOrderRequestDto
    {
        public Guid? CustomerUserId { get; set; }

        [Required]
        [MinLength(1)]
        public List<InStoreOrderDetailDto> OrderDetails { get; set; } = new List<InStoreOrderDetailDto>();

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public string? StaffNotes { get; set; }
    }
}