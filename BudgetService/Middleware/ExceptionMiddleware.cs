using System.Net;
using System.Text.Json;

namespace BudgetService.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger){ _next = next; _logger = logger; }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try { 
                await _next(ctx);
                }
            catch (Exception ex)
            {
                ctx.Response.ContentType = "application/json";
                (int code, string msg) = ex switch
                {
                    KeyNotFoundException => ((int)HttpStatusCode.NotFound, ex.Message),
                    UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, ex.Message),
                    InvalidOperationException => ((int)HttpStatusCode.BadRequest, ex.Message),
                    ArgumentException => ((int)HttpStatusCode.BadRequest, ex.Message),
                    _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.")
                };
                if (code == 500) _logger.LogError(ex, "Unhandled");
                ctx.Response.StatusCode = code;
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { statusCode = code, message = msg, timestamp = DateTime.UtcNow }));
            }
        }
    }
}