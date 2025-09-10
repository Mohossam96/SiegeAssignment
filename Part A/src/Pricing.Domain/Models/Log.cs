namespace Pricing.Domain.Models;

public class Log
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string LogLevel { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}