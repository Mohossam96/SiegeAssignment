// src/Pricing.Application/Common/Interfaces/IPriceService.cs
using Pricing.Application.Common.Models;
using Pricing.Application.Prices;

namespace Pricing.Application.Common.Interfaces;

public interface IPriceService
{
    Task<UploadPricesResult> UploadPricesAsync(Stream fileStream, CancellationToken cancellationToken);
    Task<PaginatedList<PriceDto>> GetPricesAsync(string? sku, DateOnly? validOn, string? currency, int? supplierId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<BestPriceResponse?> GetBestPriceAsync(BestPriceQuery query, CancellationToken cancellationToken);

}