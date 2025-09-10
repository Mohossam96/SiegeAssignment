namespace Pricing.Application.Prices;

public class PriceCsvRecord
{
    public int SupplierId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidTo { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal PricePerUom { get; set; }
    public int MinQty { get; set; }
}