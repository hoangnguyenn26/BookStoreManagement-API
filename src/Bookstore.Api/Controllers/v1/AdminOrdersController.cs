
using Bookstore.Application.Dtos.Orders;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookstore.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<AdminOrdersController> _logger;

        public AdminOrdersController(IOrderService orderService, ILogger<AdminOrdersController> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper to get user id
        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out Guid userId)) { throw new UnauthorizedAccessException(); }
            return userId;
        }

        // GET: api/admin/orders
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetAllOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null, // Lọc theo tên của Enum Status
            CancellationToken cancellationToken = default)
        {
            var orders = await _orderService.GetAllOrdersForAdminAsync(page, pageSize, status, cancellationToken);
            return Ok(orders);
        }

        // GET: api/admin/orders/{orderId}
        [HttpGet("{orderId:guid}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrderById(Guid orderId, CancellationToken cancellationToken)
        {
            var order = await _orderService.GetOrderByIdForAdminAsync(orderId, cancellationToken);
            if (order == null)
            {
                return NotFound(new { Message = $"Order with Id '{orderId}' not found." });
            }
            return Ok(order);
        }

        // PUT: api/admin/orders/{orderId}/status
        [HttpPut("{orderId:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusDto statusDto, CancellationToken cancellationToken)
        {
            var adminUserId = GetUserIdFromClaims();

            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(orderId, statusDto.NewStatus, adminUserId, cancellationToken);
                if (!success)
                {
                    return NotFound(new { Message = $"Order with Id '{orderId}' not found." });
                }
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId} by Admin {AdminUserId}", orderId, adminUserId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating order status.");
            }
        }
        // Các action PUT (update status) sẽ thêm sau
        // ...
    }
}