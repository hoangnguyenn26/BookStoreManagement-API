
using Bookstore.Application.Dtos;
using Bookstore.Application.Dtos.Admin.Users;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(IUserService userService, ILogger<AdminUsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: api/admin/users
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            var users = await _userService.GetAllUsersAsync(page, pageSize, role, isActive, cancellationToken);
            return Ok(users);
        }

        // GET: api/admin/users/{userId}
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserById(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // PUT: api/admin/users/{userId}/status
        [HttpPut("{userId:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusDto statusDto, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _userService.UpdateUserStatusAsync(userId, statusDto, cancellationToken);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred.");
            }
        }
    }
}