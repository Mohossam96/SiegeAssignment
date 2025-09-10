namespace Pricing.Application.Prices;

public class BestPriceQuery
{
    public string Sku { get; set; } = string.Empty;
    public int Qty { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
}