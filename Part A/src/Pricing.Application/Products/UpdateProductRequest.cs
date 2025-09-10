using System.ComponentModel.DataAnnotations;

namespace Pricing.Application.Products;

public class UpdateProductRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string Uom { get; set; } = string.Empty;
    public string HazardClass { get; set; } = string.Empty;
}