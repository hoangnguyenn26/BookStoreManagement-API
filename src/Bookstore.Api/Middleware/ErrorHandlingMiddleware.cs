
using System.Net;
using System.Text.Json;

namespace Bookstore.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Chuyển request cho middleware tiếp theo trong pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception has occurred.");

            // Mặc định là lỗi server 500
            var statusCode = HttpStatusCode.InternalServerError;
            object? responsePayload = null; // Dùng object để linh hoạt

            switch (exception)
            {
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    responsePayload = new { title = "Resource Not Found", status = (int)statusCode, detail = notFoundException.Message };
                    break;
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    responsePayload = new { title = "Validation Error", status = (int)statusCode, detail = validationException.Message };
                    break;
                case UnauthorizedAccessException unauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    responsePayload = new { title = "Unauthorized", status = (int)statusCode, detail = unauthorizedAccessException.Message };
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    var detailMessage = _env.IsDevelopment()
                        ? $"Unhandled Exception: {exception.Message} \n {exception.StackTrace}"
                        : "An internal server error has occurred.";

                    responsePayload = new { title = "Internal Server Error", status = (int)statusCode, detail = detailMessage, traceId = context.TraceIdentifier };
                    break;
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(responsePayload, jsonOptions));
        }
    }
}