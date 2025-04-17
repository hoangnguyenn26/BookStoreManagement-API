
using Bookstore.Application.Dtos.Promotions;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/promotions")]
    [Authorize(Roles = "Admin")]
    public class AdminPromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;
        private readonly ILogger<AdminPromotionsController> _logger;

        public AdminPromotionsController(IPromotionService promotionService, ILogger<AdminPromotionsController> logger)
        {
            _promotionService = promotionService;
            _logger = logger;
        }

        // GET: api/admin/promotions
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PromotionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PromotionDto>>> GetAllPromotions(CancellationToken cancellationToken)
        {
            var promotions = await _promotionService.GetAllPromotionsAsync(cancellationToken);
            return Ok(promotions);
        }

        // GET: api/admin/promotions/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PromotionDto>> GetPromotionById(Guid id, CancellationToken cancellationToken)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id, cancellationToken);
            if (promotion == null) return NotFound();
            return Ok(promotion);
        }

        // GET: api/admin/promotions/code/{code}
        [HttpGet("code/{code}")]
        [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PromotionDto>> GetPromotionByCode(string code, CancellationToken cancellationToken)
        {
            var promotion = await _promotionService.GetPromotionByCodeAsync(code, cancellationToken);
            if (promotion == null) return NotFound();
            return Ok(promotion);
        }

        // POST: api/admin/promotions
        [HttpPost]
        [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PromotionDto>> CreatePromotion([FromBody] CreatePromotionDto createDto, CancellationToken cancellationToken)
        {
            try
            {
                var createdPromo = await _promotionService.CreatePromotionAsync(createDto, cancellationToken);
                return CreatedAtAction(nameof(GetPromotionById), new { id = createdPromo.Id }, createdPromo);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating promotion with code {PromotionCode}", createDto.Code);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the promotion.");
            }
        }

        // PUT: api/admin/promotions/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePromotion(Guid id, [FromBody] UpdatePromotionDto updateDto, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _promotionService.UpdatePromotionAsync(id, updateDto, cancellationToken);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating promotion {PromotionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the promotion.");
            }
        }

        // DELETE: api/admin/promotions/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePromotion(Guid id, CancellationToken cancellationToken)
        {
            var success = await _promotionService.DeletePromotionAsync(id, cancellationToken);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}