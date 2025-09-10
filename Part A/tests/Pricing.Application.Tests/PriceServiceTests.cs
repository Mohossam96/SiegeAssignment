using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pricing.Application.Common.Interfaces;
using Pricing.Application.Prices;
using Pricing.Domain;
using Pricing.Domain.Models;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Application.Tests;

public class PriceServiceTests
{
    [Fact]
    public async Task GetBestPriceAsync_Should_ChoosePreferredSupplier_WhenPricesAreTied()
    {
        // ARRANGE

        // 1. Set up the in-memory database
        var options = new DbContextOptionsBuilder<PricingDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_PreferredSupplierTieBreaker")
            .Options;

        var context = new PricingDbContext(options);

        // Seed the in-memory database with test data
        var preferredSupplier = new Supplier { Id = 1, Name = "Preferred", Preferred = true, Active = true, LeadTimeDays = 10 };
        var nonPreferredSupplier = new Supplier { Id = 2, Name = "Non-Preferred", Preferred = false, Active = true, LeadTimeDays = 5 };

        context.Suppliers.AddRange(preferredSupplier, nonPreferredSupplier);
        context.Prices.AddRange(
            new Price { SupplierId = 1, Sku = "TIED-SKU", Currency = "USD", PricePerUom = 10.0m, ValidFrom = DateOnly.MinValue, ValidTo = DateOnly.MaxValue, Supplier = preferredSupplier },
            new Price { SupplierId = 2, Sku = "TIED-SKU", Currency = "USD", PricePerUom = 10.0m, ValidFrom = DateOnly.MinValue, ValidTo = DateOnly.MaxValue, Supplier = nonPreferredSupplier }
        );
        await context.SaveChangesAsync();

        // 2. Mock the IRateProvider
        var mockRateProvider = new Mock<IRateProvider>();
        mockRateProvider
            .Setup(p => p.GetRateAsync("USD", "USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1.0m);

        // 3. Create the service instance with our test dependencies
        var priceService = new PriceService(context, mockRateProvider.Object);
        var query = new BestPriceQuery { Sku = "TIED-SKU", Qty = 1, Date = DateOnly.FromDateTime(DateTime.Now), Currency = "USD" };

        // ACT
        var result = await priceService.GetBestPriceAsync(query, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result!.ChosenSupplier.Id.Should().Be(1); // Should be the preferred supplier's ID
        result.Reasoning.Should().Contain("'Preferred' supplier status");
    }
}