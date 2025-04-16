
using Bookstore.Application.Dtos.Orders;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // Các action PUT (update status) sẽ thêm sau
        // ...
    }
}