using Common.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Catalog.API.Services;

public class ProductCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<ProductCacheService> _logger;
    private static readonly TimeSpan ProductTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ListTtl = TimeSpan.FromMinutes(1);

    public ProductCacheService(IDistributedCache cache, ILogger<ProductCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<ProductDto?> GetProductAsync(string id)
    {
        var data = await _cache.GetStringAsync($"product:{id}");
        if (data is null)
        {
            _logger.LogInformation("[CACHE MISS] product:{Id}", id);
            return null;
        }
        _logger.LogInformation("[CACHE HIT] product:{Id}", id);
        return JsonSerializer.Deserialize<ProductDto>(data);
    }

    public async Task SetProductAsync(ProductDto product)
    {
        await _cache.SetStringAsync(
            $"product:{product.Id}",
            JsonSerializer.Serialize(product),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ProductTtl });
    }

    public async Task RemoveProductAsync(string id)
    {
        _logger.LogInformation("[CACHE CLEAR] product:{Id}", id);
        await _cache.RemoveAsync($"product:{id}");
    }

    public async Task<List<ProductDto>?> GetProductListAsync(string? search, string? category)
    {
        var key = ListKey(search, category);
        var data = await _cache.GetStringAsync(key);
        if (data is null)
        {
            _logger.LogInformation("[CACHE MISS] {Key}", key);
            return null;
        }
        _logger.LogInformation("[CACHE HIT] {Key}", key);
        return JsonSerializer.Deserialize<List<ProductDto>>(data);
    }

    public async Task SetProductListAsync(string? search, string? category, List<ProductDto> products)
    {
        await _cache.SetStringAsync(
            ListKey(search, category),
            JsonSerializer.Serialize(products),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ListTtl });
    }

    private static string ListKey(string? search, string? category) =>
        $"products:{search ?? ""}:{category ?? ""}";
}
