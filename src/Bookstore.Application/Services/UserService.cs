using AutoMapper;
using Bookstore.Application.Dtos;
using Bookstore.Application.Dtos.Admin.Users;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using LinqKit;
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

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(int page = 1, int pageSize = 10, string? roleFilter = null, bool? statusFilter = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all users for admin. RoleFilter: {Role}, StatusFilter: {Status}, Page: {Page}, PageSize: {PageSize}",
                roleFilter ?? "All", statusFilter?.ToString() ?? "All", page, pageSize);

            var predicate = PredicateBuilder.New<User>(true); // true để bắt đầu với AND

            if (statusFilter.HasValue)
            {
                predicate = predicate.And(u => u.IsActive == statusFilter.Value);
            }

            var users = await _unitOfWork.UserRepository.GetAllWithRolesAsync(predicate, page, pageSize, cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                users = users.Where(u => u.UserRoles.Any(ur => ur.Role.Name.Equals(roleFilter, StringComparison.OrdinalIgnoreCase)));
            }

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

            if (user.IsActive == statusDto.IsActive)
            {
                _logger.LogInformation("User {UserId} status already matches. No update needed.", userId);
                return true;
            }

            user.IsActive = statusDto.IsActive;
            await _unitOfWork.UserRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated status for user {UserId} to IsActive={IsActive}", userId, statusDto.IsActive);
            return true;
        }
    }
}