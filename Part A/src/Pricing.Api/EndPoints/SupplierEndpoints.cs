using Pricing.Api.Filters;
using Pricing.Application.Common.Interfaces; 
using Pricing.Application.Suppliers;

namespace Pricing.Api.Endpoints;

public static class SupplierEndpoints
{
    public static void MapSupplierEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suppliers");

        group.MapGet("/", async (ISupplierService service, CancellationToken ct) =>
        {
            var suppliers = await service.GetAllAsync(ct);
            return Results.Ok(suppliers);
        });

        group.MapGet("/{id:int}", async (int id, ISupplierService service, CancellationToken ct) =>
        {
            var supplier = await service.GetByIdAsync(id, ct);
            return supplier is not null ? Results.Ok(supplier) : Results.NotFound();
        });

        group.MapPost("/", async (CreateSupplierRequest request, ISupplierService service, CancellationToken ct) =>
        {
            var createdSupplier = await service.CreateAsync(request, ct);
            if(createdSupplier == null)
            {
                return Results.BadRequest();
            }
            return Results.Ok(createdSupplier);
        }).AddEndpointFilter<ValidationFilter<CreateSupplierRequest>>();

        group.MapPut("/{id:int}", async (int id, UpdateSupplierRequest request, ISupplierService service, CancellationToken ct) =>
        {
            var success = await service.UpdateAsync(id, request, ct);
            return success ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, ISupplierService service, CancellationToken ct) =>
        {
            var success = await service.DeleteAsync(id, ct);
            return success ? Results.NoContent() : Results.NotFound();
        });
    }

}
