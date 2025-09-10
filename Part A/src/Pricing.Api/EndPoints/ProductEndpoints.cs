using Pricing.Api.Filters;
using Pricing.Application.Common.Interfaces;
using Pricing.Application.Products;

namespace Pricing.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products");

        group.MapGet("/", async (IProductService service, CancellationToken ct) => await service.GetAllAsync(ct));

        group.MapGet("/{id:guid}", async (Guid id, IProductService service, CancellationToken ct) =>
        {
            var product = await service.GetByIdAsync(id, ct);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        }).WithName("GetProductById");

        group.MapPost("/", async (CreateProductRequest request, IProductService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return Results.CreatedAtRoute("GetProductById", new { id = created.Id }, created);
        }).AddEndpointFilter<ValidationFilter<CreateProductRequest>>();

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IProductService service, CancellationToken ct) =>
        {
            return await service.UpdateAsync(id, request, ct) ? Results.NoContent() : Results.NotFound();
        }).AddEndpointFilter<ValidationFilter<UpdateProductRequest>>();

        group.MapDelete("/{id:guid}", async (Guid id, IProductService service, CancellationToken ct) =>
        {
            return await service.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound();
        });
    }
}