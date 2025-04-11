
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")] 
    [ApiVersion("1.0")] 
    public class HealthCheckController : ControllerBase
    {
        // GET /api/v1/healthcheck
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        public IActionResult GetHealth()
        {
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }
    }
}