namespace Pricing.Application.Common.Interfaces;
public interface IRateProvider
{
    Task<decimal?> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken);
}