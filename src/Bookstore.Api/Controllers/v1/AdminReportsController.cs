
using Bookstore.Application.Dtos.Admin.Reports;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<AdminReportsController> _logger;

        public AdminReportsController(IReportService reportService, ILogger<AdminReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // GET: api/admin/reports/revenue
        [HttpGet("revenue")]
        [ProducesResponseType(typeof(RevenueReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RevenueReportDto>> GetRevenueReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            CancellationToken cancellationToken)
        {
            // Kiểm tra ngày hợp lệ cơ bản
            if (startDate == default || endDate == default || startDate > endDate)
            {
                return BadRequest("Invalid date range provided. Please provide valid 'startDate' and 'endDate'.");
            }

            try
            {
                var report = await _reportService.GetRevenueReportAsync(startDate, endDate, cancellationToken);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate revenue report for period {StartDate} to {EndDate}", startDate, endDate);
                return StatusCode(StatusCodes.Status500InternalServerError, "Could not generate the revenue report.");
            }
        }

        // GET: api/admin/reports/bestsellers
        [HttpGet("bestsellers")]
        [ProducesResponseType(typeof(IEnumerable<BestsellerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BestsellerDto>>> GetBestsellersReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int top = 5,
            CancellationToken cancellationToken = default)
        {
            if (startDate == default || endDate == default || startDate > endDate)
            {
                return BadRequest("Invalid date range provided.");
            }
            if (top <= 0) top = 5;

            try
            {
                var report = await _reportService.GetBestsellersReportAsync(startDate, endDate, top, cancellationToken);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate bestsellers report for period {StartDate} to {EndDate}", startDate, endDate);
                return StatusCode(StatusCodes.Status500InternalServerError, "Could not generate the bestsellers report.");
            }
        }

        // GET: api/admin/reports/stock
        [HttpGet("stock")]
        [ProducesResponseType(typeof(IEnumerable<LowStockBookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LowStockBookDto>>> GetLowStockReport(
            [FromQuery] int threshold = 5,
            CancellationToken cancellationToken = default)
        {
            if (threshold < 0) threshold = 0;

            try
            {
                var report = await _reportService.GetLowStockReportAsync(threshold, cancellationToken);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate low stock report with threshold {LowStockThreshold}", threshold);
                return StatusCode(StatusCodes.Status500InternalServerError, "Could not generate the low stock report.");
            }
        }

    }
}