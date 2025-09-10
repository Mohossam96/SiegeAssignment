using System.Net;
using System.Net.Http.Json;
using Pricing.Api.Tests;
using Pricing.Application.Prices;
using Xunit;

namespace Pricing.Api.Tests;

public class BestPriceQueryIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BestPriceQueryIntegrationTests(CustomWebApplicationFactory factory)
    {
        // The factory now handles all the database setup and seeding!
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBestPrice_ReturnsCorrectWinner_BasedOnExample()
    {
        // ARRANGE
        // This URL matches the exact scenario from the project requirements
        var url = "/api/pricing/best?sku=ABC123&qty=120&currency=EUR&date=2025-09-01";

        // ACT
        var response = await _client.GetAsync(url);

        // ASSERT
        // 1. Check for a successful HTTP status code
        response.EnsureSuccessStatusCode();

        // 2. Deserialize the JSON response into our DTO
        var bestPrice = await response.Content.ReadFromJsonAsync<BestPriceResponse>();

        // 3. Assert that the content is correct
        Assert.NotNull(bestPrice);
        Assert.Equal(2, bestPrice.ChosenSupplier.Id); // Supplier #2 should win
        Assert.Equal(9.2m, bestPrice.UnitPrice); // 10.00 USD * 0.92 rate = 9.20 EUR
        Assert.Equal(1104.0m, bestPrice.TotalPrice); // 9.20 * 120
    }
}