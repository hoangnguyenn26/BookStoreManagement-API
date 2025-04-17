// src/Bookstore.Api/Controllers/Admin/AdminDashboardController.cs
using Bookstore.Application.Dtos.Admin.Dashboard;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(IReportService reportService, ILogger<AdminDashboardController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // GET: api/admin/dashboard/summary
        [HttpGet("summary")]
        [ProducesResponseType(typeof(AdminDashboardSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdminDashboardSummaryDto>> GetSummary(
            [FromQuery] int lowStockThreshold = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var summary = await _reportService.GetAdminDashboardSummaryAsync(lowStockThreshold, cancellationToken);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get admin dashboard summary.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Could not retrieve dashboard summary data.");
            }
        }
    }
}