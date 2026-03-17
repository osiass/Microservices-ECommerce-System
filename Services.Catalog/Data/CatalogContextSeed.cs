using Catalog.API.Entities;

namespace Catalog.API.Data;

public static class CatalogContextSeed
{
    public static async Task SeedAsync(CatalogContext context)
    {
        // Eğer tabloda hiç ürün yoksa örnek ürünleri ekle
        if (!context.Products.Any())
        {
            context.Products.AddRange(new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Gaming Mouse", Description = "RGB, 16000 DPI", Price = 500, Stock = 100 },
                new Product { Id = Guid.NewGuid(), Name = "Mechanical Keyboard", Description = "Blue Switch", Price = 1200, Stock = 50 },
                new Product { Id = Guid.NewGuid(), Name = "Monitor", Description = "144Hz, 1ms", Price = 4500, Stock = 20 }
            });

            await context.SaveChangesAsync();
        }
    }
}