namespace FinalProjectV3.Middlewares
{
    public class UnauthorizedResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UnauthorizedResponseMiddleware> _logger;
        public UnauthorizedResponseMiddleware(RequestDelegate next, ILogger<UnauthorizedResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized &&
                !context.Response.HasStarted)
            {
                _logger.LogWarning("401 Unauthorized response returned for request: {Path}", context.Request.Path);

                context.Response.ContentType = "application/json";
                var result = new { Error = "Unauthorized" };
                await context.Response.WriteAsJsonAsync(result);
            }
        }
    }

}
