// src/Pricing.Application/Prices/BestPriceResponse.cs
namespace Pricing.Application.Prices;

public class BestPriceResponse
{
    public SupplierInfo ChosenSupplier { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;

    public class SupplierInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}