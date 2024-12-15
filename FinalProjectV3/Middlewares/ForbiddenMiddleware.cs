using System.Text.Json;

namespace FinalProjectV3.Middlewares
{
    public class ForbiddenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ForbiddenMiddleware> _logger;

        public ForbiddenMiddleware(RequestDelegate next, ILogger<ForbiddenMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                _logger.LogWarning("403 Forbidden response returned for request: {Path}", context.Request.Path);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        message = "Forbidden",
                        details = "You do not have permission to access this resource."
                    })
                );
            }
        }
    }

}
