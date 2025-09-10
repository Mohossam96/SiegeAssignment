// tests/Pricing.Api.Tests/CustomWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Pricing.Domain;
using Pricing.Domain.Models;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Find and remove the real database registration from Program.cs
            var dbContextOptions = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PricingDbContext>));

            if (dbContextOptions != null)
            {
                services.Remove(dbContextOptions);
            }

            // Add a new in-memory database provider for testing
            services.AddDbContext<PricingDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Build the service provider to seed the database
            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<PricingDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();

                // Seed the database with our test data
                SeedData(db);
            }
        });
    }

    private void SeedData(PricingDbContext db)
    {
        // Clear data from previous tests
        db.Prices.RemoveRange(db.Prices);
        db.Suppliers.RemoveRange(db.Suppliers);
        db.SaveChanges();

        // Add the specific data needed for our BestPrice test
        db.Suppliers.AddRange(
            new Supplier { Id = 1, Name = "Euro Supplies", Preferred = true, Active = true, LeadTimeDays = 5 },
            new Supplier { Id = 2, Name = "Global Parts", Preferred = false, Active = true, LeadTimeDays = 15 }
        );
        db.Prices.AddRange(
            new Price { SupplierId = 1, Sku = "ABC123", Currency = "EUR", PricePerUom = 9.50m, ValidFrom = new DateOnly(2025, 8, 1), ValidTo = new DateOnly(2025, 12, 31) },
            new Price { SupplierId = 2, Sku = "ABC123", Currency = "USD", PricePerUom = 10.00m, ValidFrom = new DateOnly(2025, 7, 1), ValidTo = new DateOnly(2025, 10, 31) }
        );
        db.SaveChanges();
    }
}