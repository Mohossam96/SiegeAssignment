// src/Pricing.Application/Common/Interfaces/ISupplierService.cs
using Pricing.Application.Suppliers;

namespace Pricing.Application.Common.Interfaces;

public interface ISupplierService
{
    Task<IEnumerable<SupplierDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<SupplierDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}