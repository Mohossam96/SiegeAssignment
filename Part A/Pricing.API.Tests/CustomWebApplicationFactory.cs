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
        // This method runs to configure the test server
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
                // Use a unique name to ensure tests are isolated
                options.UseInMemoryDatabase("InMemoryDbForIntegrationTesting");
            });

            // Build the service provider to get a DbContext instance
            var sp = services.BuildServiceProvider();

            // Seed the database with our specific test data
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<PricingDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();

                // Call our seeding logic
                SeedData(db);
            }
        });
    }

    private void SeedData(PricingDbContext db)
    {
        // Clear out any data from a previous test run
        db.Prices.RemoveRange(db.Prices);
        db.Suppliers.RemoveRange(db.Suppliers);
        db.SaveChanges();

        // Add the specific Suppliers and Prices needed for the BestPrice test
        db.Suppliers.AddRange(
            new Supplier { Id = 1, Name = "Euro Supplies", Preferred = true, Active = true, LeadTimeDays = 5, Country = "Germany" },
            new Supplier { Id = 2, Name = "Global Parts", Preferred = false, Active = true, LeadTimeDays = 15, Country = "USA" }
        );
        db.Prices.AddRange(
            new Price { Id = Guid.NewGuid(), SupplierId = 1, Sku = "ABC123", Currency = "EUR", MinQty = 100, PricePerUom = 9.50m, ValidFrom = new DateOnly(2025, 8, 1), ValidTo = new DateOnly(2025, 12, 31) },
            new Price { Id = Guid.NewGuid(), SupplierId = 2, Sku = "ABC123", Currency = "USD", MinQty = 50, PricePerUom = 10.00m, ValidFrom = new DateOnly(2025, 7, 1), ValidTo = new DateOnly(2025, 10, 31) }
        );
        db.SaveChanges();
    }
}