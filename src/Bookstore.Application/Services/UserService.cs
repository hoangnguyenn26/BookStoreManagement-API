
using AutoMapper;
using Bookstore.Application.Dtos;
using Bookstore.Application.Dtos.Admin.Users;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;


namespace Bookstore.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all users for admin. Page: {Page}, PageSize: {PageSize}", page, pageSize);
            var users = await _unitOfWork.UserRepository.GetAllWithRolesAsync(page, pageSize, cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching user details for admin. UserId: {UserId}", userId);
            var user = await _unitOfWork.UserRepository.GetByIdWithRolesAsync(userId, cancellationToken: cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for admin request.", userId);
                return null;
            }
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto statusDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to update status for user {UserId} to IsActive={IsActive}", userId, statusDto.IsActive);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken, isTracking: true);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for status update.", userId);
                return false;
            }

            if (user.IsActive == statusDto.IsActive) return true;

            user.IsActive = statusDto.IsActive;

            await _unitOfWork.UserRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated status for user {UserId} to IsActive={IsActive}", userId, statusDto.IsActive);
            return true;
        }

        //public async Task<bool> UpdateUserRolesAsync(Guid userId, UpdateUserRolesDto rolesDto, CancellationToken cancellationToken = default)
        //{
        //    _logger.LogInformation("Attempting to update roles for user {UserId}", userId);
        //    var user = await _unitOfWork.UserRepository.GetByIdWithRolesAsync(userId, tracking: true, cancellationToken: cancellationToken);
        //    if (user == null) return false;

        //    var newRoles = new List<Role>();
        //    foreach (var roleName in rolesDto.RoleNames.Distinct())
        //    {
        //        var roleEntity = await _unitOfWork.RoleRepository.GetByNameAsync(roleName, cancellationToken);
        //        if (roleEntity == null) throw new ValidationException($"Role '{roleName}' not found.");
        //        newRoles.Add(roleEntity);
        //    }

        //    var rolesToRemove = user.UserRoles.Where(ur => !newRoles.Any(nr => nr.Id == ur.RoleId)).ToList();
        //    if (rolesToRemove.Any()) _unitOfWork.Context.UserRoles.RemoveRange(rolesToRemove); // Xóa trực tiếp từ Context DbSet

        //    var rolesToAdd = newRoles.Where(nr => !user.UserRoles.Any(ur => ur.RoleId == nr.Id)).ToList();
        //    foreach (var roleToAdd in rolesToAdd)
        //    {
        //        _unitOfWork.Context.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleToAdd.Id });
        //    }

        //    await _unitOfWork.SaveChangesAsync(cancellationToken);
        //    _logger.LogInformation("Successfully updated roles for user {UserId}", userId);
        //    return true;
        //}
    }
}