namespace Pricing.Domain.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Uom { get; set; } = string.Empty;
        public string HazardClass { get; set; } = string.Empty;
    }
}
