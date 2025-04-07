namespace Bookstore.Domain.Entities
{
    // Không cần kế thừa BaseEntity vì đây là bảng nối thuần túy
    public class UserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        // Navigation properties (Quan trọng cho EF Core để hiểu mối quan hệ)
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}