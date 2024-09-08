using System.Net;
using System.Text.Json;

namespace GitHubFreshdeskIntegration.WebAPI.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Default to Internal Server Error (500)
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred. Please try again later.";

            // You can add special handling for specific exception types
            if (exception is UnauthorizedAccessException)
            {
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = "Unauthorized access.";
            }
            else if (exception is ArgumentException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Invalid request data.";
            }

            // Create the error response
            var errorResponse = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                // Include detailed message and exception type in development environments only
                Detailed = _env.IsDevelopment() ? exception.Message : null,
                ExceptionType = _env.IsDevelopment() ? exception.GetType().Name : null
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

}
