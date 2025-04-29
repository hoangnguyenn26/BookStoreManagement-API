using Bookstore.Application.Dtos.Inventory;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookstore.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/inventory")]
    [Authorize(Roles = "Admin,Staff")]
    public class AdminInventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<AdminInventoryController> _logger;

        public AdminInventoryController(IInventoryService inventoryService, ILogger<AdminInventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        // Helper lấy UserId
        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out Guid userId)) { throw new UnauthorizedAccessException("User identifier not found."); }
            return userId;
        }

        // POST: api/admin/inventory/adjust
        [HttpPost("adjust")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<int>> AdjustStock([FromBody] AdjustInventoryRequestDto adjustDto, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var newQuantity = await _inventoryService.AdjustStockManuallyAsync(userId, adjustDto, cancellationToken);
                return Ok(newQuantity);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized inventory adjustment attempt.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual stock adjustment for Book {BookId}", adjustDto.BookId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adjusting stock.");
            }
        }
    }
}