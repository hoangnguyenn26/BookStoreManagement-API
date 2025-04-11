namespace Bookstore.Domain.Entities
{
    public class UserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}