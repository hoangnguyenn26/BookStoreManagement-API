using Bookstore.Application.Dtos.Inventory;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/inventory")]
    [Authorize(Roles = "Admin,Staff")]
    public class AdminInventoryController : BaseApiController
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<AdminInventoryController> _logger;

        public AdminInventoryController(IInventoryService inventoryService, ILogger<AdminInventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
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

        // GET: api/admin/inventory/history
        [HttpGet("history")]
        [ProducesResponseType(typeof(PagedInventoryLogResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedInventoryLogResult>> GetInventoryHistory(
            [FromQuery] Guid? bookId = null,
            [FromQuery] InventoryReason? reason = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? performedByUserId = null,
            [FromQuery] Guid? orderId = null,
            [FromQuery] Guid? stockReceiptId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                return BadRequest("Start date cannot be after end date.");
            }

            try
            {
                var history = await _inventoryService.GetInventoryHistoryAsync(
                    bookId, reason, startDate, endDate, performedByUserId, orderId, stockReceiptId,
                    page, pageSize, cancellationToken);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inventory history.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching inventory history.");
            }
        }
    }
}