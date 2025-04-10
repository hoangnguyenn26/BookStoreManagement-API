
namespace Bookstore.Application.Settings
{
    public class JwtSettings
    {
        public string Key { get; set; } = null!;       // Khóa bí mật để ký token (phải đủ dài và phức tạp)
        public string Issuer { get; set; } = null!;    // Đơn vị phát hành token (ví dụ: tên ứng dụng hoặc domain)
        public string Audience { get; set; } = null!; // Đối tượng dự kiến sử dụng token (ví dụ: tên ứng dụng hoặc domain)
        public int DurationInMinutes { get; set; }     // Thời gian hiệu lực của token (tính bằng phút)
    }
}