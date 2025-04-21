
using Bookstore.Application.Dtos;
using Bookstore.Application.Dtos.Admin.Users;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto statusDto, CancellationToken cancellationToken = default);
        //Task<bool> UpdateUserRolesAsync(Guid userId, UpdateUserRolesDto rolesDto, CancellationToken cancellationToken = default); // Optional
    }
}