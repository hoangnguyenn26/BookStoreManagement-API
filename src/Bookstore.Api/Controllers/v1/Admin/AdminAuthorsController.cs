using Bookstore.Api.Controllers.v1;
using Bookstore.Application.Dtos.Authors;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/authors")]
    [Authorize(Roles = "Admin,Staff")]
    public class AdminAuthorsController : ControllerBase
    {
        private readonly IAuthorService _authorService;
        private readonly ILogger<AdminAuthorsController> _logger;

        public AdminAuthorsController(IAuthorService authorService, ILogger<AdminAuthorsController> logger)
        {
            _authorService = authorService;
            _logger = logger;
        }

        // POST: api/admin/authors
        [HttpPost]
        [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthorDto>> CreateAuthor([FromBody] CreateAuthorDto createDto, CancellationToken cancellationToken)
        {
            try
            {
                var createdAuthor = await _authorService.CreateAuthorAsync(createDto, cancellationToken);
                // Trả về action GET của public controller
                return CreatedAtAction(nameof(AuthorsController.GetAuthorById), "Authors", new { id = createdAuthor.Id, version = "1.0" }, createdAuthor);
            }
            catch (ValidationException ex) { return BadRequest(new { Message = ex.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Error creating author."); return StatusCode(500); }
        }

        // PUT: api/admin/authors/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAuthor(Guid id, [FromBody] UpdateAuthorDto updateDto, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _authorService.UpdateAuthorAsync(id, updateDto, cancellationToken);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (ValidationException ex) { return BadRequest(new { Message = ex.Message }); }
            catch (NotFoundException) { return NotFound(); }
            catch (Exception ex) { _logger.LogError(ex, "Error updating author {AuthorId}.", id); return StatusCode(500); }
        }

        // DELETE: api/admin/authors/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAuthor(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _authorService.DeleteAuthorAsync(id, cancellationToken);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (NotFoundException) { return NotFound(); }
            catch (Exception ex) { _logger.LogError(ex, "Error deleting author {AuthorId}.", id); return StatusCode(500); }
        }
    }
}