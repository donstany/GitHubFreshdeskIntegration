using Refit;
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
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred. Please try again later.";


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
            else if (exception is ApiException)
            {
                var ex = exception as ApiException;
                statusCode = (int)ex.StatusCode;
                message = $"API Error from 3-rd party app: {ex.Message}; {ex.Content}";
            }


            var errorResponse = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
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
