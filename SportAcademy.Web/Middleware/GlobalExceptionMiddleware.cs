using System.Text.Json;

namespace SportAcademy.Web.Middleware
{
    // Last-resort safety net for exceptions that escape the MediatR pipeline (ExceptionHandlingBehavior
    // handles those). Anything thrown in middleware, filters, or controller code before _mediator.Send
    // previously hit ASP.NET Core's bare default behavior - this ensures every request gets a
    // consistent JSON error response instead of a raw 500/stack trace.
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
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);

                if (context.Response.HasStarted)
                {
                    throw;
                }

                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    isSuccess = false,
                    operationType = "UnhandledException",
                    statusCode = StatusCodes.Status500InternalServerError,
                    message = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred. Please try again later.",
                    errors = (object?)null,
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        }
    }
}
