using Microsoft.EntityFrameworkCore;
using Pricing.Domain;
using Pricing.Domain.Models;
using System.Reflection;

namespace Pricing.Infrastructure.Persistence;

public class PricingDbContext : DbContext
{
    public PricingDbContext(DbContextOptions<PricingDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Price> Prices { get; set; }
    public DbSet<Log> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Sku).HasMaxLength(50);
            entity.Property(p => p.Name).HasMaxLength(200);
            entity.Property(p => p.Uom).HasMaxLength(10);
        });

       
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(200);
            entity.Property(s => s.Country).HasMaxLength(100);
        });

        
        modelBuilder.Entity<Price>(entity =>
        {
            entity.HasIndex(p => new { p.SupplierId, p.Sku });
        });

        base.OnModelCreating(modelBuilder);
    }
}