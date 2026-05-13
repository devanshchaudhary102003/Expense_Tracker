using System.Diagnostics;

namespace ApiGateway.Middleware;

public class GatewayLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GatewayLoggingMiddleware> _logger;

    public GatewayLoggingMiddleware(RequestDelegate next, ILogger<GatewayLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        var sw = Stopwatch.StartNew();
        var method = ctx.Request.Method;
        var path   = ctx.Request.Path;

        _logger.LogInformation("→ {Method} {Path}", method, path);
        try
        {
            await _next(ctx);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("← {Method} {Path} => {Status} in {Ms} ms",
                method, path, ctx.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }
}
