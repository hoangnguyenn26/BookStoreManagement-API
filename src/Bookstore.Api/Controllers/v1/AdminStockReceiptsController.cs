
using Bookstore.Application.Dtos.StockReceipts;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/stock-receipts")]
    [Authorize(Roles = "Admin,Staff")]
    public class AdminStockReceiptsController : BaseApiController
    {
        private readonly IStockReceiptService _stockReceiptService;
        private readonly ILogger<AdminStockReceiptsController> _logger;

        public AdminStockReceiptsController(IStockReceiptService stockReceiptService, ILogger<AdminStockReceiptsController> logger)
        {
            _stockReceiptService = stockReceiptService;
            _logger = logger;
        }

        // GET: api/admin/stock-receipts
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<StockReceiptDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StockReceiptDto>>> GetAllReceipts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var receipts = await _stockReceiptService.GetAllStockReceiptsAsync(page, pageSize, cancellationToken);
            return Ok(receipts);
        }

        // GET: api/admin/stock-receipts/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(StockReceiptDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StockReceiptDto>> GetReceiptById(Guid id, CancellationToken cancellationToken)
        {
            var receipt = await _stockReceiptService.GetStockReceiptByIdAsync(id, cancellationToken);
            if (receipt == null) return NotFound();
            return Ok(receipt);
        }


        // POST: api/admin/stock-receipts
        [HttpPost]
        [ProducesResponseType(typeof(StockReceiptDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StockReceiptDto>> CreateReceipt([FromBody] CreateStockReceiptDto createDto, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var createdReceipt = await _stockReceiptService.CreateStockReceiptAsync(createDto, userId, cancellationToken);
                // Trả về 201 Created
                return CreatedAtAction(nameof(GetReceiptById), new { id = createdReceipt.Id }, createdReceipt);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stock receipt.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the stock receipt.");
            }
        }
    }
}