using System.ComponentModel.DataAnnotations;

namespace Pricing.Application.Suppliers;

public class CreateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool Active { get; set; }
    public bool Preferred { get; set; }
    [Range(0, int.MaxValue,ErrorMessage ="Lead Time Days cannot be negative")]
    public int LeadTimeDays { get; set; }
}