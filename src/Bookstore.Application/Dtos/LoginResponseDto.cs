
namespace Bookstore.Application.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; } = null!;
    }
}