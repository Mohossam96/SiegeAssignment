namespace Pricing.Application.Common.Services; 

using Pricing.Application.Common.Interfaces;

public class InMemoryRateProvider : IRateProvider
{
    private static readonly Dictionary<(string From, string To), decimal> _rates = new()
    {
        { ("USD", "EUR"), 0.92m },
        { ("EUR", "USD"), 1.09m },
        { ("USD", "GBP"), 0.80m },
        { ("GBP", "USD"), 1.25m }
    };

    public Task<decimal?> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<decimal?>(1.0m);
        }

        if (_rates.TryGetValue((fromCurrency.ToUpper(), toCurrency.ToUpper()), out var rate))
        {
            return Task.FromResult<decimal?>(rate);
        }

        return Task.FromResult<decimal?>(null);
    }
}