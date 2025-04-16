
using Bookstore.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Dtos.Orders
{
    public class UpdateOrderStatusDto
    {
        [Required]
        [EnumDataType(typeof(OrderStatus))]
        public OrderStatus NewStatus { get; set; }
    }
}