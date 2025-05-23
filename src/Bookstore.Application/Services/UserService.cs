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

        // src/Bookstore.Application/Services/UserService.cs
        // ... (using, constructor, các phương thức khác) ...

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(
            int page = 1,
            int pageSize = 10,
            string? roleFilter = null,
            bool? isActiveFilter = null,
            string? searchQuery = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching users for admin. Page: {Page}, PageSize: {PageSize}, Role: {Role}, Active: {Active}, Search: {Search}",
                page, pageSize, roleFilter ?? "All", isActiveFilter?.ToString() ?? "All", searchQuery ?? "None");

            // --- Xây dựng Filter Expression ---
            var predicate = PredicateBuilder.New<User>(true);

            if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter.ToLower() != "all")
            {
                _logger.LogWarning("Role filter in GetAllUsersAsync is complex and not fully implemented without specific repository method for roles.");
            }

            if (isActiveFilter.HasValue)
            {
                predicate = predicate.And(u => u.IsActive == isActiveFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var term = searchQuery.Trim().ToLower();
                predicate = predicate.And(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(term))
                );
            }
            var users = await _unitOfWork.UserRepository.ListAsync(
                filter: predicate,
                orderBy: q => q.OrderBy(u => u.UserName),
                includeProperties: "UserRoles.Role",
                page: page,
                pageSize: pageSize,
                isTracking: false,
                cancellationToken: cancellationToken
            );

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