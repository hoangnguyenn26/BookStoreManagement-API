// src/Bookstore.Api/Controllers/Admin/AdminReviewsController.cs
using Bookstore.Application.Dtos.Reviews;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/reviews")]
    [Authorize(Roles = "Admin")]
    public class AdminReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<AdminReviewsController> _logger;

        public AdminReviewsController(IReviewService reviewService, ILogger<AdminReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        // GET: api/admin/reviews
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetAllReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var reviews = await _reviewService.GetAllReviewsForAdminAsync(page, pageSize, cancellationToken);
            return Ok(reviews);
        }

        // DELETE: api/admin/reviews/{reviewId}
        [HttpDelete("{reviewId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview(Guid reviewId, CancellationToken cancellationToken)
        {
            var success = await _reviewService.DeleteReviewAsync(reviewId, cancellationToken);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}