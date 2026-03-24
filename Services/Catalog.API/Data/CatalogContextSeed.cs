using Catalog.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Data;

public static class CatalogContextSeed
{
    public static async Task SeedAsync(CatalogContext context)
    {
        var allProducts = await context.Products.ToListAsync();
        foreach (var p in allProducts)
        {
            if (p.Name == "Gaming Mouse") p.Category = "Mouse";
            if (p.Name == "Mechanical Keyboard") p.Category = "Klavye";
            if (p.Name == "Monitor") p.Category = "Monitör";
        }
        await context.SaveChangesAsync();

        //Eğer hiç ürün yoksa yeni ürünleri ekle
        if (!context.Products.Any())
        {
            context.Products.AddRange(new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Gaming Mouse", Category = "Mouse", Description = "RGB, 16000 DPI", Price = 500, StockQuantity = 100, ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=500" },
                new Product { Id = Guid.NewGuid(), Name = "Mechanical Keyboard", Category = "Klavye", Description = "Blue Switch", Price = 1200, StockQuantity = 50, ImageUrl = "https://images.unsplash.com/photo-1511467687858-23d96c32e4ae?w=500" },
                new Product { Id = Guid.NewGuid(), Name = "Monitor", Category = "Monitör", Description = "144Hz, 1ms", Price = 4500, StockQuantity = 20, ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=500" }
            });

            await context.SaveChangesAsync();
        }
    }
}