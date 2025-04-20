// src/Bookstore.Api/Controllers/Staff/StaffOrdersController.cs
using Bookstore.Application.Dtos.Orders;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.Staff
{
    [ApiController]
    [Route("api/staff/orders")]
    [Authorize(Roles = "Staff,Admin")]

    public class StaffOrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<StaffOrdersController> _logger;

        public StaffOrdersController(IOrderService orderService, ILogger<StaffOrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // POST: api/staff/orders
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<OrderDto>> CreateInStoreOrder([FromBody] CreateInStoreOrderRequestDto createDto, CancellationToken cancellationToken)
        {
            try
            {
                var staffUserId = GetUserIdFromClaims();
                var createdOrder = await _orderService.CreateInStoreOrderAsync(staffUserId, createDto, cancellationToken);

                return StatusCode(StatusCodes.Status201Created, createdOrder);
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
                _logger.LogWarning(ex, "Unauthorized in-store order creation attempt.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating in-store order by staff {StaffId}.", GetUserIdFromClaims());
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the in-store order.");
            }
        }
    }
}