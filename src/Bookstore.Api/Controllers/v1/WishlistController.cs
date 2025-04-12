using Bookstore.Application.Dtos.Wishlists;
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
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService ?? throw new ArgumentNullException(nameof(wishlistService));
        }


        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("User identifier not found in token.");
            }
            return userId;
        }

        // GET: api/v1/wishlist
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<WishlistItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<WishlistItemDto>>> GetWishlist(CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            var wishlist = await _wishlistService.GetUserWishlistAsync(userId, cancellationToken);
            return Ok(wishlist);
        }

        // POST: api/v1/wishlist/{bookId}
        [HttpPost("{bookId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddToWishlist(Guid bookId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            var success = await _wishlistService.AddToWishlistAsync(userId, bookId, cancellationToken);

            if (!success)
            {
                return NotFound(new { Message = "Book not found or could not be added to wishlist." });
            }

            return NoContent();
        }

        // DELETE: api/v1/wishlist/{bookId}
        [HttpDelete("{bookId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFromWishlist(Guid bookId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            var success = await _wishlistService.RemoveFromWishlistAsync(userId, bookId, cancellationToken);

            if (!success)
            {
                return NotFound(new { Message = "Item not found in wishlist." });
            }

            return NoContent();
        }

        // DELETE: api/v1/wishlist/items/{itemId}
        [HttpDelete("items/{itemId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveWishlistItemById(Guid itemId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdFromClaims();
            var success = await _wishlistService.RemoveFromWishlistByIdAsync(userId, itemId, cancellationToken);
            if (!success)
            {
                return NotFound(new { Message = "Wishlist item not found or you don't have permission." });
            }
            return NoContent();
        }
    }
}