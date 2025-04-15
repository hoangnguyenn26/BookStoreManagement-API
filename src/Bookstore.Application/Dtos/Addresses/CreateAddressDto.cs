namespace Bookstore.Application.Dtos.Addresses
{
    public class CreateAddressDto
    {
        public string Street { get; set; } = null!;
        public string Village { get; set; } = null!;
        public string District { get; set; } = null!;
        public string City { get; set; } = null!;
        public bool IsDefault { get; set; } = false;
    }
}