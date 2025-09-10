using System.Net;
using System.Net.Http.Json;
using Pricing.Application.Prices;
using Xunit;

namespace Pricing.Api.Tests;

public class BestPriceQueryTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BestPriceQueryTests(CustomWebApplicationFactory factory)
    {
        // The factory now handles all the database setup and seeding!
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBestPrice_ReturnsCorrectWinner_BasedOnExample()
    {
        // ARRANGE
        var url = "/api/pricing/best?sku=ABC123&qty=120&currency=EUR&date=2025-09-01";

        // ACT
        var response = await _client.GetAsync(url);

        // ASSERT
        response.EnsureSuccessStatusCode();

        var bestPrice = await response.Content.ReadFromJsonAsync<BestPriceResponse>();
        Assert.NotNull(bestPrice);
        Assert.Equal(2, bestPrice.ChosenSupplier.Id);
        Assert.Equal(9.2m, bestPrice.UnitPrice);
    }
}