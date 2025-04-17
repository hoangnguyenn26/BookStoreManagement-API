// src/Bookstore.Api/Controllers/v1/DashboardController.cs
using Bookstore.Application.Dtos.Dashboard;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/home")]
    [ApiVersion("1.0")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        // GET: api/v1/home/dashboard
        [HttpGet("dashboard")]
        [AllowAnonymous] // Cho phép truy cập công khai
        [ProducesResponseType(typeof(HomeDashboardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HomeDashboardDto>> GetHomeDashboard(CancellationToken cancellationToken)
        {
            try
            {
                var dashboardData = await _dashboardService.GetHomeDashboardAsync(cancellationToken);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get home dashboard data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Could not retrieve dashboard data.");
            }
        }
    }
}