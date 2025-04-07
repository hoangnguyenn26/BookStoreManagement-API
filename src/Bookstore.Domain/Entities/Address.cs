namespace Bookstore.Domain.Entities
{
    public class Address : BaseEntity
    {
        public Guid UserId { get; set; } // Khóa ngoại tới User
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? State { get; set; }
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;
        public bool IsDefault { get; set; } = false;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}