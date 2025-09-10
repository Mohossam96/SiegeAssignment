using System.ComponentModel.DataAnnotations;

namespace Pricing.Application.Products;

public class CreateProductRequest
{
    [Required]
    [StringLength(50,ErrorMessage ="Sku can't exceed 50 letters ")]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [StringLength(200,ErrorMessage ="Name can't exceed 200 letters")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(10, ErrorMessage = "Uom can't exceed 10 letters")]
    public string Uom { get; set; } = string.Empty;
    public string HazardClass { get; set; } = string.Empty;
}