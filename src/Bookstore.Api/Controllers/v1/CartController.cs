using Bookstore.Application.Dtos.Carts;
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
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("User identifier not found in token.");
            }
            return userId;
        }

        // GET: api/v1/cart
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CartItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCart(CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            var cart = await _cartService.GetUserCartAsync(userId, cancellationToken);
            return Ok(cart);
        }

        // POST: api/v1/cart/items
        [HttpPost("items")]
        [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartItemDto>> AddOrUpdateItem([FromBody] AddCartItemDto addItemDto, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            try
            {
                var result = await _cartService.AddOrUpdateCartItemAsync(userId, addItemDto, cancellationToken);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the cart.");
            }
        }

        // PUT: api/v1/cart/items/{bookId}
        [HttpPut("items/{bookId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateItemQuantity(Guid bookId, [FromBody] UpdateCartItemDto updateDto, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            try
            {
                var success = await _cartService.UpdateCartItemQuantityAsync(userId, bookId, updateDto, cancellationToken);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the cart item.");
            }
        }

        // DELETE: api/v1/cart/items/{bookId}
        [HttpDelete("items/{bookId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItem(Guid bookId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            var success = await _cartService.RemoveCartItemAsync(userId, bookId, cancellationToken);
            if (!success)
            {
                return NotFound(new { Message = "Item not found in cart." });
            }
            return NoContent();
        }

        // DELETE: api/v1/cart
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            await _cartService.ClearUserCartAsync(userId, cancellationToken);
            return NoContent();
        }
    }
}