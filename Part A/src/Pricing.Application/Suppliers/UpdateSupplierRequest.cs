namespace Pricing.Application.Suppliers;

public class UpdateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool Active { get; set; }
    public bool Preferred { get; set; }
    public int LeadTimeDays { get; set; }
}