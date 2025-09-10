using Microsoft.AspNetCore.Mvc;
using Pricing.Application.Common.Interfaces;
using Pricing.Application.Prices;

namespace Pricing.Api.Endpoints;

public static class PricingQueryEndpoints
{
    public static void MapPricingQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pricing");

        group.MapGet("/best", async (
            [FromServices] IPriceService service,
            [AsParameters] BestPriceQuery query,
            CancellationToken ct) =>
        {
            var result = await service.GetBestPriceAsync(query, ct);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });
    }
}