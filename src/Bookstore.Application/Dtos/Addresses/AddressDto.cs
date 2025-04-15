namespace Bookstore.Application.Dtos.Addresses
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public string Street { get; set; } = null!;
        public string Village { get; set; } = null!;
        public string District { get; set; } = null!;
        public string City { get; set; } = null!;
        public bool IsDefault { get; set; }
    }
}