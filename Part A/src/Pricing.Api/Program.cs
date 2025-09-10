using Microsoft.EntityFrameworkCore;
using Pricing.Api.Endpoints;
using Pricing.Api.Middleware;
using Pricing.Application.Common.Interfaces;
using Pricing.Application.Common.Services;
using Pricing.Application.Prices;
using Pricing.Application.Products;
using Pricing.Application.Suppliers;
using Pricing.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PricingDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddMemoryCache();

// Add services to the container.
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPriceService, PriceService>();
builder.Services.AddSingleton<IRateProvider, InMemoryRateProvider>();
var app = builder.Build();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<PricingDbContext>();
        await ApplicationDbContextInitialiser.InitialiseAsync(context);
    }
}
// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.MapSupplierEndpoints();
app.MapProductEndpoints();
app.MapPriceEndpoints();
app.MapPricingQueryEndpoints();


app.Run();
