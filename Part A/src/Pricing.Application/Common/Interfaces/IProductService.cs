﻿using Pricing.Application.Products;

namespace Pricing.Application.Common.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}