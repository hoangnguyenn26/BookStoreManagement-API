
using Bookstore.Application.Dtos.Orders;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper lấy UserId
        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("User identifier not found in token.");
            }
            return userId;
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

                // Trả về 201 Created với action để lấy đơn hàng vừa tạo
                // Cần tạo action GetOrderByIdForUser trước (Ngày 25)
                // return CreatedAtAction(nameof(GetOrderByIdForUser), new { orderId = createdOrder.Id, version = \"1.0\" }, createdOrder);

                // Tạm thời trả về Ok hoặc Created với object
                return StatusCode(StatusCodes.Status201Created, createdOrder);
            }
            catch (ValidationException ex)
            {
                // Lỗi nghiệp vụ (vd: hết hàng, giỏ rỗng)
                return BadRequest(new { Message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                // Lỗi không tìm thấy tài nguyên (vd: địa chỉ)
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex) // Lỗi lấy UserId từ token
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
            var userId = GetUserIdFromClaims(); // Lấy userId của người dùng đang đăng nhập
            var orders = await _orderService.GetUserOrdersAsync(userId, page, pageSize, cancellationToken);
            // TODO: Thêm thông tin phân trang vào Header hoặc Response Body nếu cần
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

        // Các action GET, PUT (cancel) sẽ được thêm ở các ngày sau
    }
}