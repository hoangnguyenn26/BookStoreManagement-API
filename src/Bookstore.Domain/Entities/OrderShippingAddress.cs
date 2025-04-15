
namespace Bookstore.Domain.Entities
{
    public class OrderShippingAddress
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Street { get; set; } = null!;
        public string? Village { get; set; }
        public string District { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? RecipientName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}