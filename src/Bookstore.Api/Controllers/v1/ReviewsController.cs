
using Bookstore.Application.Dtos.Reviews;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/books/{bookId:guid}/reviews")]
    [ApiVersion("1.0")]
    public class ReviewsController : BaseApiController
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        // GET: api/v1/books/{bookId}/reviews
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ReviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetBookReviews(
            Guid bookId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var reviews = await _reviewService.GetApprovedReviewsForBookAsync(bookId, page, pageSize, cancellationToken);
            return Ok(reviews);
        }

        // POST: api/v1/books/{bookId}/reviews
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ReviewDto>> AddBookReview(Guid bookId, [FromBody] CreateReviewDto createDto, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var createdReview = await _reviewService.AddReviewAsync(userId, bookId, createDto, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, createdReview);
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
                _logger.LogWarning(ex, "Unauthorized review submission attempt.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review for book {BookId} by user {UserId}", bookId, GetUserIdFromClaims());
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the review.");
            }
        }
    }
}