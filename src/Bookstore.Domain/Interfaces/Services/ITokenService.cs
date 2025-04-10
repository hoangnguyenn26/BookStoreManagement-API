// src/Bookstore.Domain/Interfaces/Services/ITokenService.cs
using Bookstore.Domain.Entities; // Namespace chứa User

namespace Bookstore.Domain.Interfaces.Services
{
    public interface ITokenService
    {
        string CreateToken(User user, IList<string> roles); // Nhận User và danh sách Roles
    }
}