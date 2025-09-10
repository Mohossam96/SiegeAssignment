using Microsoft.AspNetCore.Mvc;
using Pricing.Application.Common.Interfaces;

namespace Pricing.Api.Endpoints;

public static class PriceEndpoints
{
    public static void MapPriceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/prices");

        group.MapPost("/upload", async (IFormFile file, [FromServices] IPriceService service, CancellationToken ct) =>
        {
            if (file.Length == 0) return Results.BadRequest("File is empty.");

            var result = await service.UploadPricesAsync(file.OpenReadStream(), ct);

            if (!result.IsSuccess)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { "CsvValidation", result.Errors.ToArray() }
        });
            }

            return Results.Ok("Prices uploaded successfully.");
        }).DisableAntiforgery();

        group.MapGet("/", async (
            [FromServices] IPriceService service,
            string? sku,
            DateOnly? validOn,
            string? currency,
            int? supplierId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken ct = default) =>
        {
            var prices = await service.GetPricesAsync(sku, validOn, currency, supplierId, pageNumber, pageSize, ct);
            return Results.Ok(prices);
        });
    }
}