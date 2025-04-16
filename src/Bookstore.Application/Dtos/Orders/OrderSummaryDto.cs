
using Bookstore.Domain.Enums;

namespace Bookstore.Application.Dtos.Orders
{
    public class OrderSummaryDto
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderType OrderType { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? UserName { get; set; }
        public int ItemCount { get; set; }
    }
}