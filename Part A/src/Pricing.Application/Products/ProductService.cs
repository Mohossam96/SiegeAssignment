using Microsoft.EntityFrameworkCore;
using Pricing.Application.Common.Interfaces;
using Pricing.Domain;
using Pricing.Domain.Models;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Application.Products;

public class ProductService : IProductService
{
    private readonly PricingDbContext _context;

    public ProductService(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Uom = p.Uom,
                HazardClass = p.HazardClass
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return product == null ? null : new ProductDto 
        { 
            Id = id,
            Sku = product.Sku,
            Name = product.Name,
            Uom = product.Uom,
            HazardClass = product.HazardClass
        };
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Uom = request.Uom,
            HazardClass = request.HazardClass
        };
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new ProductDto 
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Uom = product.Uom,
            HazardClass = product.HazardClass
        };
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product == null) return false;

        product.Name = request.Name;
        product.Uom = request.Uom;
        product.HazardClass = request.HazardClass;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}