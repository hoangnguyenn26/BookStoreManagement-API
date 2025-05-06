using Bookstore.Application.Dtos.Books;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _logger = logger;
        }

        // GET: api/v1/books
        // GET: api/v1/books?categoryId={guid}&search=sapiens&page=1&pageSize=10
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        {
            var books = await _bookService.GetAllBooksAsync(categoryId, search, page, pageSize, cancellationToken);
            return Ok(books);
        }

        // GET: api/v1/books/{id}
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookDto>> GetBookById(Guid id, CancellationToken cancellationToken)
        {
            var book = await _bookService.GetBookByIdAsync(id, cancellationToken);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }

        // POST: api/v1/books
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookDto>> CreateBook([FromBody] CreateBookDto createBookDto, CancellationToken cancellationToken)
        {
            // Validation tự động
            try
            {
                var createdBook = await _bookService.CreateBookAsync(createBookDto, cancellationToken);
                return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id, version = "1.0" }, createdBook);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the book.");
            }
        }

        // PUT: api/v1/books/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBook(Guid id, [FromBody] UpdateBookDto updateBookDto, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _bookService.UpdateBookAsync(id, updateBookDto, cancellationToken);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log lỗi
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the book.");
            }
        }

        // DELETE: api/v1/books/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBook(Guid id, CancellationToken cancellationToken)
        {
            var success = await _bookService.DeleteBookAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        // POST:
        [HttpPost("{id:guid}/image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadCoverImage(Guid id, IFormFile imageFile, CancellationToken cancellationToken)
        {
            // Kiểm tra file cơ bản
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { Message = "No image file uploaded." });
            }
            try
            {
                var success = await _bookService.UpdateBookCoverImageAsync(id, imageFile, cancellationToken);
                if (!success)
                {
                    return NotFound(new { Message = $"Book with Id '{id}' not found or failed to save image." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading cover image for book {BookId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while uploading the image.");
            }
        }
    }
}