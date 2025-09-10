namespace Pricing.Application.Prices;

public class PriceDto
{
    public Guid Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidTo { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal PricePerUom { get; set; }
    public int MinQty { get; set; }
}