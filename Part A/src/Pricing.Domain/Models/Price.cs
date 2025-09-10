using System.ComponentModel.DataAnnotations.Schema;

namespace Pricing.Domain.Models
{
    public class Price
    {
        public Guid Id { get; set; }
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;
        public string Sku { get; set; } = string.Empty;
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }
        public string Currency { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 4)")]
        public decimal PricePerUom { get; set; }
        public int MinQty { get; set; }
        
    }
}
