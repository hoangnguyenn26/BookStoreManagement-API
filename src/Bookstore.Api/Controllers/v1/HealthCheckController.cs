
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
        [ProducesResponseType(StatusCodes.Status200OK)] // Khai báo kiểu trả về thành công
        public IActionResult GetHealth()
        {
            // Trả về một response đơn giản để xác nhận API đang chạy
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }
    }
}