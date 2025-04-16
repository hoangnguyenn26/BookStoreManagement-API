
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Application.Dtos.Orders
{
    public class CreateOrderRequestDto
    {
        // Thông tin lấy từ Cart trên server, chỉ cần thông tin bổ sung

        [Required]
        public Guid ShippingAddressId { get; set; } // ID của địa chỉ User đã lưu (trong bảng Addresses)

        public string? PromotionCode { get; set; } // Mã khuyến mãi (tùy chọn)

    }
}