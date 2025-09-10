using Pricing.Domain;
using Pricing.Domain.Models;
using Pricing.Infrastructure.Persistence;
using System.Net;
using System.Text.Json;

namespace Pricing.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred.");

            await using (var scope = scopeFactory.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PricingDbContext>();
                var log = new Log
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    LogLevel = "Error",
                    Timestamp = DateTime.UtcNow
                };
                await dbContext.Logs.AddAsync(log);
                await dbContext.SaveChangesAsync();
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An unexpected internal server error has occurred. The issue has been logged."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}