using Microsoft.EntityFrameworkCore;
using Pricing.Application.Common.Interfaces;
using Pricing.Domain.Models;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Application.Suppliers;
public class SupplierService : ISupplierService
{
    private readonly PricingDbContext _context;

    public SupplierService(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SupplierDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                Country = s.Country,
                Active = s.Active,
                Preferred = s.Preferred,
                LeadTimeDays = s.LeadTimeDays
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SupplierDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier == null)
        {
            return null;
        }

        return new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Country = supplier.Country,
            Active = supplier.Active,
            Preferred = supplier.Preferred,
            LeadTimeDays = supplier.LeadTimeDays
        };
    }
    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        // Basic validation

        var supplier = new Supplier
        {
            Name = request.Name,
            Country = request.Country,
            Active = request.Active,
            Preferred = request.Preferred,
            LeadTimeDays = request.LeadTimeDays
        };

        await _context.Suppliers.AddAsync(supplier, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);


        return new SupplierDto 
        { 
            Id = supplier.Id,
            Name = supplier.Name,
            Country = supplier.Country,
            Active = supplier.Active,
            Preferred = supplier.Preferred,
            LeadTimeDays = supplier.LeadTimeDays
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers.FindAsync(new object[] { id }, cancellationToken);

        if (supplier == null)
        {
            return false;
        }

        supplier.Name = request.Name;
        supplier.Country = request.Country;
        supplier.Active = request.Active;
        supplier.Preferred = request.Preferred;
        supplier.LeadTimeDays = request.LeadTimeDays;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers.FindAsync(new object[] { id }, cancellationToken);

        if (supplier == null)
        {
            return false; 
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}