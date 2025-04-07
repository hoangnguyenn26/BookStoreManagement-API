
namespace Bookstore.Application.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        // Không bao gồm PasswordHash!
        // Có thể thêm các thông tin khác nếu cần, ví dụ: Roles
        // public List<string> Roles { get; set; } = new List<string>();
    }
}