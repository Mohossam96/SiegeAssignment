// src/Pricing.Application/Prices/PriceService.cs
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Common.Interfaces;
using Pricing.Application.Common.Models;
using Pricing.Domain;
using Pricing.Domain.Models;
using Pricing.Infrastructure.Persistence;
using System.Globalization;

namespace Pricing.Application.Prices;

public class PriceService : IPriceService
{
    private readonly PricingDbContext _context;
    private readonly IRateProvider _rateProvider;

    public PriceService(PricingDbContext context, IRateProvider rateProvider)
    {
        _context = context;
        _rateProvider = rateProvider;
    }

    public async Task<UploadPricesResult> UploadPricesAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        var records = ParseCsv(fileStream);
        if (records is null || !records.Any())
        {
            return UploadPricesResult.Failure(new List<string> { "CSV file is empty or invalid." });
        }

        var validationErrors = await ValidateRecordsAsync(records, cancellationToken);
        if (validationErrors.Any())
        {
            return UploadPricesResult.Failure(validationErrors);
        }

        await SaveRecordsAsync(records, cancellationToken);

        return UploadPricesResult.Success();
    }

    private List<PriceCsvRecord>? ParseCsv(Stream fileStream)
    {
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture);
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, config);
            return csv.GetRecords<PriceCsvRecord>().ToList();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<List<string>> ValidateRecordsAsync(List<PriceCsvRecord> records, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        // 1. Fetch all necessary existing data in bulk to avoid N+1 queries
        var supplierIdsInCsv = records.Select(r => r.SupplierId).Distinct().ToList();
        var skusInCsv = records.Select(r => r.Sku).Distinct().ToList();

        // With this corrected line:
        var existingSupplierIds = (await _context.Suppliers.Where(s => supplierIdsInCsv.Contains(s.Id)).Select(s => s.Id).ToListAsync(cancellationToken)).ToHashSet();
        var existingProductSkus = (await _context.Products.Where(p => skusInCsv.Contains(p.Sku)).Select(p => p.Sku).ToListAsync(cancellationToken)).ToHashSet();

        // Fetch all potentially overlapping prices from the database at once
        var relevantDbPrices = await _context.Prices
            .Where(p => supplierIdsInCsv.Contains(p.SupplierId) && skusInCsv.Contains(p.Sku))
            .ToListAsync(cancellationToken);

        // 2. Validate each record
        for (var i = 0; i < records.Count; i++)
        {
            var record = records[i];
            var rowNumber = i + 2; // +1 for 0-index, +1 for header row

            if (!existingSupplierIds.Contains(record.SupplierId))
                errors.Add($"Row {rowNumber}: Supplier with Id '{record.SupplierId}' does not exist.");

            if (!existingProductSkus.Contains(record.Sku))
                errors.Add($"Row {rowNumber}: Product with Sku '{record.Sku}' does not exist.");

            if (record.ValidFrom > record.ValidTo)
                errors.Add($"Row {rowNumber}: ValidFrom date cannot be after ValidTo date.");

            // 3. Check for overlaps with existing DB data
            var dbOverlap = relevantDbPrices.Any(p =>
                p.SupplierId == record.SupplierId &&
                p.Sku == record.Sku &&
                record.ValidFrom <= p.ValidTo &&
                record.ValidTo >= p.ValidFrom);

            if (dbOverlap)
                errors.Add($"Row {rowNumber}: Price for ({record.SupplierId}, {record.Sku}) has a date range that overlaps with an existing price in the database.");

            // 4. Check for overlaps within the CSV file itself
            var internalOverlap = records.Take(i).Any(prev =>
                prev.SupplierId == record.SupplierId &&
                prev.Sku == record.Sku &&
                record.ValidFrom <= prev.ValidTo &&
                record.ValidTo >= prev.ValidFrom);

            if (internalOverlap)
                errors.Add($"Row {rowNumber}: Price for ({record.SupplierId}, {record.Sku}) has a date range that overlaps with another entry in the same CSV file.");
        }

        return errors;
    }

    private async Task SaveRecordsAsync(List<PriceCsvRecord> records, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var newPrices = records.Select(r => new Price
            {
                SupplierId = r.SupplierId,
                Sku = r.Sku,
                ValidFrom = r.ValidFrom,
                ValidTo = r.ValidTo,
                Currency = r.Currency,
                PricePerUom = r.PricePerUom,
                MinQty = r.MinQty
            });

            await _context.Prices.AddRangeAsync(newPrices, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw; // Re-throw the exception to be handled by a global error handler
        }
    }

    public async Task<PaginatedList<PriceDto>> GetPricesAsync(string? sku, DateOnly? validOn, string? currency, int? supplierId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Prices
            .Include(p => p.Supplier)
            .AsNoTracking();

        // --- Apply Filters ---
        if (!string.IsNullOrEmpty(sku))
        {
            query = query.Where(p => p.Sku == sku);
        }
        if (supplierId.HasValue)
        {
            query = query.Where(p => p.SupplierId == supplierId.Value);
        }
        if (validOn.HasValue)
        {
            query = query.Where(p => p.ValidFrom <= validOn.Value && p.ValidTo >= validOn.Value);
        }
        if (!string.IsNullOrEmpty(currency))
        {
            query = query.Where(p => p.Currency.ToUpper() == currency.ToUpper());
        }

        // Get the total count of items that match the filters (before pagination)
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project to DTO
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PriceDto
            {
                Id = p.Id,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier.Name,
                Sku = p.Sku,
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo,
                Currency = p.Currency,
                PricePerUom = p.PricePerUom,
                MinQty = p.MinQty
            })
            .ToListAsync(cancellationToken);

        
        return new PaginatedList<PriceDto>(items, totalCount, pageNumber, pageSize);
    }
    public async Task<BestPriceResponse?> GetBestPriceAsync(BestPriceQuery query, CancellationToken cancellationToken)
    {
        // 1. Find all potential candidate prices from the database (no change here)
        var candidates = await _context.Prices
            .Include(p => p.Supplier)
            .Where(p => p.Sku == query.Sku &&
                         p.Supplier.Active &&
                         query.Date >= p.ValidFrom &&
                         query.Date <= p.ValidTo &&
                         query.Qty >= p.MinQty)
            .ToListAsync(cancellationToken);

        if (!candidates.Any()) return null;

        // 2. Normalize prices to the requested currency (no change here)
        var enrichedCandidates = new List<(Price Price, decimal ConvertedRate)>();
        foreach (var candidate in candidates)
        {
            var rate = await _rateProvider.GetRateAsync(candidate.Currency, query.Currency, cancellationToken);
            if (rate.HasValue)
            {
                enrichedCandidates.Add((candidate, candidate.PricePerUom * rate.Value));
            }
        }

        if (!enrichedCandidates.Any()) return null;

        // 3. Apply the sorting and tie-breaker logic to find the winner (no change here)
        var winner = enrichedCandidates
            .OrderBy(c => c.ConvertedRate)
            .ThenByDescending(c => c.Price.Supplier.Preferred)
            .ThenBy(c => c.Price.Supplier.LeadTimeDays)
            .ThenBy(c => c.Price.Supplier.Id)
            .FirstOrDefault();

        // 4. Construct the response with DYNAMIC REASONING
        var winningPrice = winner.Price;
        var winningRate = winner.ConvertedRate;

        return new BestPriceResponse
        {
            ChosenSupplier = new BestPriceResponse.SupplierInfo
            {
                Id = winningPrice.Supplier.Id,
                Name = winningPrice.Supplier.Name
            },
            UnitPrice = winningRate,
            TotalPrice = winningRate * query.Qty,
            Currency = query.Currency,
            Reasoning = GenerateReasoning(winner, enrichedCandidates, candidates.Count)
        };
    }

    private string GenerateReasoning((Price Price, decimal ConvertedRate) winner, List<(Price Price, decimal ConvertedRate)> candidates, int initialCandidateCount)
    {
        string baseReason = $"Selected from {initialCandidateCount} valid supplier(s). ";
        var winningPrice = winner.ConvertedRate;
        var priceFormatted = winningPrice.ToString("F2"); // Format to 2 decimal places

        // Check for price ties
        var priceTies = candidates.Where(c => c.ConvertedRate == winningPrice).ToList();
        if (priceTies.Count == 1)
        {
            return baseReason + $"Winning factor: Lowest unit price of {priceFormatted}.";
        }

        // Price tie exists, check for preference tie-breaker
        var winnerIsPreferred = winner.Price.Supplier.Preferred;
        var preferenceTies = priceTies.Where(c => c.Price.Supplier.Preferred == winnerIsPreferred).ToList();
        if (preferenceTies.Count == 1 && winnerIsPreferred)
        {
            return baseReason + $"A price tie of {priceFormatted} was broken by 'Preferred' supplier status.";
        }

        // Tie on price and preference, check for lead time tie-breaker
        var winningLeadTime = winner.Price.Supplier.LeadTimeDays;
        var leadTimeTies = preferenceTies.Where(c => c.Price.Supplier.LeadTimeDays == winningLeadTime).ToList();
        if (leadTimeTies.Count == 1)
        {
            return baseReason + $"A price tie of {priceFormatted} was broken by shortest lead time of {winningLeadTime} days.";
        }

        // Final tie-breaker must be Supplier ID
        return baseReason + $"A price tie of {priceFormatted} was broken by lowest Supplier ID ({winner.Price.Supplier.Id}).";
    }
}