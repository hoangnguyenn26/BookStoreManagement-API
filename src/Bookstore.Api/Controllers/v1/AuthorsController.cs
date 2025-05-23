// src/Bookstore.Api/Controllers/v1/AuthorsController.cs
using Bookstore.Application.Dtos.Authors;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _authorService;
        public AuthorsController(IAuthorService authorService) { _authorService = authorService; }

        // GET: api/v1/authors
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<AuthorDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(
                    [FromQuery] string? search,
                    [FromQuery] int page = 1,
                    [FromQuery] int pageSize = 10,
                    CancellationToken cancellationToken = default)
        {
            return Ok(await _authorService.GetAllAuthorsAsync(search, page, pageSize, cancellationToken));
        }

        // GET: api/v1/authors/{id}
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorDto>> GetAuthorById(Guid id, CancellationToken cancellationToken)
        {
            var author = await _authorService.GetAuthorByIdAsync(id, cancellationToken);
            if (author == null) return NotFound();
            return Ok(author);
        }
    }
}