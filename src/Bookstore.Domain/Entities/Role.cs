
namespace Bookstore.Domain.Entities
{
    public class Role : BaseEntity // Kế thừa BaseEntity
    {
        public string Name { get; set; } = null!;

        // Navigation property (optional for now)
        // public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}