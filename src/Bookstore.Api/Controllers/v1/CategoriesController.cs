
using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        // GET: api/v1/categories
        [HttpGet]
        [AllowAnonymous] 
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
            return Ok(categories);
        }

        // GET: api/v1/categories/{id}
        [HttpGet("{id:guid}")]
        [AllowAnonymous] 
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id, CancellationToken cancellationToken)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        // POST: api/v1/categories
        [HttpPost]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto, CancellationToken cancellationToken)
        {
            var createdCategory = await _categoryService.CreateCategoryAsync(createCategoryDto, cancellationToken);
            return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id, version = "1.0" }, createdCategory);
        }

        // PUT: api/v1/categories/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateCategoryDto, CancellationToken cancellationToken)
        {
            var success = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto, cancellationToken);
            if (!success)
            {
                return NotFound();
            }
            return NoContent(); 
        }

        // DELETE: api/v1/categories/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
        {
            var success = await _categoryService.DeleteCategoryAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound(); 
            }
            return NoContent(); 
        }
    }
}