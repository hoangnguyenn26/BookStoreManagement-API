
using Bookstore.Application.Dtos.Suppliers;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/suppliers")]
    [Authorize(Roles = "Admin")]
    public class AdminSuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly ILogger<AdminSuppliersController> _logger;

        public AdminSuppliersController(ISupplierService supplierService, ILogger<AdminSuppliersController> logger)
        {
            _supplierService = supplierService;
            _logger = logger;
        }

        // GET: api/admin/suppliers
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SupplierDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAllSuppliers(CancellationToken cancellationToken)
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync(cancellationToken);
            return Ok(suppliers);
        }

        // GET: api/admin/suppliers/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SupplierDto>> GetSupplierById(Guid id, CancellationToken cancellationToken)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id, cancellationToken);
            if (supplier == null) return NotFound();
            return Ok(supplier);
        }

        // POST: api/admin/suppliers
        [HttpPost]
        [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SupplierDto>> CreateSupplier([FromBody] CreateSupplierDto createDto, CancellationToken cancellationToken)
        {
            try
            {
                var createdSupplier = await _supplierService.CreateSupplierAsync(createDto, cancellationToken);
                return CreatedAtAction(nameof(GetSupplierById), new { id = createdSupplier.Id }, createdSupplier);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred.");
            }
        }

        // PUT: api/admin/suppliers/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierDto updateDto, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _supplierService.UpdateSupplierAsync(id, updateDto, cancellationToken);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier {SupplierId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred.");
            }
        }

        // DELETE: api/admin/suppliers/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSupplier(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _supplierService.DeleteSupplierAsync(id, cancellationToken);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier {SupplierId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred.");
            }
        }
    }
}