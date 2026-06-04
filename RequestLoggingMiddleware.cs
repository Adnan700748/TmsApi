using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate correlation id
        var correlationId = Guid.NewGuid()
            .ToString("N")[..8];

        // Add response header BEFORE next()
        context.Response.Headers["X-Correlation-Id"] =
            correlationId;

        // Start timer
        var stopwatch = Stopwatch.StartNew();

        // Entry log
        _logger.LogInformation(
            "Request {Method} {Path} CorrelationId={CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        // Continue pipeline
        await _next(context);

        // Stop timer
        stopwatch.Stop();

        // Exit log
        _logger.LogInformation(
            "Response {StatusCode} Elapsed={ElapsedMs}ms CorrelationId={CorrelationId}",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}