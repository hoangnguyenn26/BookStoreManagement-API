
using Bookstore.Application.Dtos.Orders;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // POST: api/v1/orders
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<OrderDto>> CreateOnlineOrder([FromBody] CreateOrderRequestDto createOrderDto, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var createdOrder = await _orderService.CreateOnlineOrderAsync(userId, createOrderDto, cancellationToken);

                return CreatedAtAction(nameof(GetMyOrderById), new { orderId = createdOrder.Id, version = "1.0" }, createdOrder);

            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt during order creation.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating an online order.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while creating your order.");
            }
        }

        // GET: api/v1/orders
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetMyOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserIdFromClaims();
            var orders = await _orderService.GetUserOrdersAsync(userId, page, pageSize, cancellationToken);

            return Ok(orders);
        }

        // GET: api/v1/orders/{orderId}
        [HttpGet("{orderId:guid}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<OrderDto>> GetMyOrderById(Guid orderId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            var order = await _orderService.GetOrderByIdForUserAsync(userId, orderId, cancellationToken);

            if (order == null)
            {
                return NotFound(new { Message = $"Order with Id '{orderId}' not found or you don't have permission." });
            }

            return Ok(order);
        }

        // PUT: api/v1/orders/{orderId}/cancel
        [HttpPut("{orderId:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelMyOrder(Guid orderId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            try
            {
                var success = await _orderService.CancelOrderAsync(userId, orderId, cancellationToken);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = "Order not found or you don't have permission." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized cancellation attempt for order {OrderId}", orderId);
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId} for user {UserId}", orderId, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while cancelling the order.");
            }
        }
    }
}