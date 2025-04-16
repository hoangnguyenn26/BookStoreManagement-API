// src/Bookstore.Application/Dtos/Orders/OrderDetailDto.cs
using Bookstore.Application.Dtos.Books; // Cần BookDto

namespace Bookstore.Application.Dtos.Orders
{
    public class OrderDetailDto
    {
        public Guid BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Giá tại thời điểm mua
        public BookDto Book { get; set; } = null!; // Thông tin sách chi tiết
    }
}