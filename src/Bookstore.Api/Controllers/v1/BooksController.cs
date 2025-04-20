
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

        public BooksController(IBookService bookService)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
        }

        // GET: api/v1/books
        // GET: api/v1/books?categoryId={guid}&page=1&pageSize=20
        [HttpGet]
        [AllowAnonymous] // Ai cũng xem được sách
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks(
            [FromQuery] Guid? categoryId, // <<-- THÊM THAM SỐ NÀY
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10, // Giữ lại phân trang nếu đã có
                                           // Thêm các tham số khác nếu cần: sortBy, sortDirection...
            CancellationToken cancellationToken = default)
        {
            // Gọi service với tham số categoryId đã thêm
            var books = await _bookService.GetAllBooksAsync(categoryId, page, pageSize, cancellationToken);
            // TODO: Xử lý thông tin phân trang trả về (vd: trong Header) nếu cần
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
    }
}