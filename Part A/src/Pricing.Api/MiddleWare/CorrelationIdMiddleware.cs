namespace Pricing.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeaderKey = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();

        
        context.TraceIdentifier = correlationId!;


        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append(CorrelationIdHeaderKey, correlationId);
            return Task.CompletedTask;
        });

        using (_logger.BeginScope("{@CorrelationId}", correlationId))
        {
            await _next(context);
        }
    }
}