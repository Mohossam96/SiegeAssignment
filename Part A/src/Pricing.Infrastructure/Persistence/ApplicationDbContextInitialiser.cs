using Microsoft.EntityFrameworkCore;
using Pricing.Domain;
using Pricing.Domain.Models;

namespace Pricing.Infrastructure.Persistence;

public static class ApplicationDbContextInitialiser
{
    public static async Task InitialiseAsync(PricingDbContext context)
    {
        
        if (await context.Suppliers.AnyAsync())
        {
            return;
        }
        var suppliers = new[]
        {
            new Supplier {  Name = "Euro Supplies Co.", Country = "Germany", Active = true, Preferred = true, LeadTimeDays = 5 },
            new Supplier {  Name = "Global Parts Inc.", Country = "USA", Active = true, Preferred = false, LeadTimeDays = 15 },
            new Supplier {  Name = "National Components", Country = "Germany", Active = true, Preferred = false, LeadTimeDays = 5 },
            new Supplier {  Name = "Asia Direct", Country = "China", Active = false, Preferred = false, LeadTimeDays = 30 }
        };
        await context.Suppliers.AddRangeAsync(suppliers);

        // Create Products
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Sku = "ABC123", Name = "Industrial Grade Adhesive", Uom = "LITER", HazardClass = "3" },
            new Product { Id = Guid.NewGuid(), Sku = "XYZ777", Name = "Precision Steel Bearing", Uom = "PIECE", HazardClass = "N/A" },
            new Product { Id = Guid.NewGuid(), Sku = "QWE456", Name = "Rubber Sealant", Uom = "KG", HazardClass = "N/A" }
        };
        await context.Products.AddRangeAsync(products);

        // Save changes to the database
        await context.SaveChangesAsync();
    }
}