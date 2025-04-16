
namespace Bookstore.Domain.Entities
{
    public class Address : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Street { get; set; } = null!;
        public string Village { get; set; } = null!;
        public string District { get; set; } = null!;
        public string City { get; set; } = null!;

        public bool IsDefault { get; set; } = false;

        public virtual User User { get; set; } = null!;
    }
}