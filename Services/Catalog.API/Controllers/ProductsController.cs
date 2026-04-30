using Catalog.API.Data;
using Catalog.API.Entities;
using Catalog.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Common.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly CatalogContext _context;
    private readonly Common.EventBus.IEventBus _eventBus;
    private readonly ILogger<ProductsController> _logger;
    private readonly ProductCacheService _cache;

    public ProductsController(CatalogContext context, Common.EventBus.IEventBus eventBus, ILogger<ProductsController> logger, ProductCacheService cache)
    {
        _context = context;
        _eventBus = eventBus;
        _logger = logger;
        _cache = cache;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDto>))]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(string? search, string? category)
    {
        var cached = await _cache.GetProductListAsync(search, category);
        if (cached != null) return Ok(cached);

        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        var products = await query.ToListAsync();
        var result = products.Select(MapToDto).ToList();

        await _cache.SetProductListAsync(search, category, result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(string id)
    {
        if (!Guid.TryParse(id, out var guidId)) return BadRequest("Invalid product ID");

        var cached = await _cache.GetProductAsync(id);
        if (cached != null) return Ok(cached);

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == guidId);

        if (product == null) return NotFound();

        var dto = MapToDto(product);
        await _cache.SetProductAsync(dto);

        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProductDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] ProductDto productDto)
    {
        var product = new Product
        {
            Name = productDto.Name!,
            Category = productDto.Category!,
            Description = productDto.Description ?? "",
            Price = productDto.Price,
            StockQuantity = productDto.StockQuantity,
            Features = productDto.Features ?? new List<string>()
        };

        if (productDto.ImageFile != null)
        {
            product.ImageUrl = await SaveImageAsync(productDto.ImageFile);
        }

        // Ek resimler
        if (productDto.AdditionalImageFiles != null)
        {
            foreach (var file in productDto.AdditionalImageFiles)
            {
                product.ImageUrls.Add(await SaveImageAsync(file));
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _ = PublishProductCreated(product);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, MapToDto(product));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        if (!Guid.TryParse(id, out var guidId)) return BadRequest("Invalid product ID");
        
        var product = await _context.Products.FindAsync(guidId);
        if (product == null) return NotFound($"Product with ID {id} not found in database.");

        // İlişkili yorumları sil (Cascading)
        var comments = await _context.ProductComments.Where(c => c.ProductId == guidId).ToListAsync();
        if (comments.Any()) _context.ProductComments.RemoveRange(comments);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        await _cache.RemoveProductAsync(id);
        _ = PublishProductDeleted(id);

        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(string id, [FromForm] ProductDto productDto)
    {
        if (!Guid.TryParse(id, out var guidId)) return BadRequest();
        
        var product = await _context.Products.FindAsync(guidId);
        if (product == null) return NotFound();

        product.Name = productDto.Name!;
        product.Category = productDto.Category!;
        product.Description = productDto.Description ?? "";
        product.Price = productDto.Price;
        product.StockQuantity = productDto.StockQuantity;
        product.Features = productDto.Features ?? new List<string>();

        if (productDto.ImageFile != null)
        {
            product.ImageUrl = await SaveImageAsync(productDto.ImageFile);
        }

        // Ek resimler
        if (productDto.AdditionalImageFiles != null)
        {
            foreach (var file in productDto.AdditionalImageFiles)
            {
                product.ImageUrls.Add(await SaveImageAsync(file));
            }
        }

        await _context.SaveChangesAsync();

        await _cache.RemoveProductAsync(id);
        _ = PublishProductUpdated(product);

        return NoContent();
    }

    [HttpPost("{id}/rate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RatingDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RatingDto>> RateProduct(string id, [FromBody] int score)
    {
        if (!Guid.TryParse(id, out var guidId)) return BadRequest();
        var product = await _context.Products.FindAsync(guidId);
        if (product == null) return NotFound();

        double totalScore = (product.Rating * product.ReviewCount) + score;
        product.ReviewCount++;
        product.Rating = totalScore / product.ReviewCount;
        
        await _context.SaveChangesAsync();
        return Ok(new RatingDto { Rating = product.Rating, ReviewCount = product.ReviewCount });
    }

    [HttpGet("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CommentDto>))]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(string id)
    {
        if (!Guid.TryParse(id, out var guidId)) return BadRequest();
        var comments = await _context.ProductComments
            .AsNoTracking()
            .Where(c => c.ProductId == guidId)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();

        return Ok(comments.Select(c => new CommentDto 
        { 
            Id = c.Id, 
            UserName = c.UserName, 
            Content = c.Content, 
            CreatedDate = c.CreatedDate 
        }).ToList());
    }

    [HttpPost("{id}/comments")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CommentDto))]
    public async Task<ActionResult<CommentDto>> AddComment(string id, CommentDto commentDto)
    {
        if (!Guid.TryParse(id, out var guidId)) return BadRequest();
        
        var comment = new ProductComment
        {
            ProductId = guidId,
            UserName = commentDto.UserName,
            Content = commentDto.Content,
            CreatedDate = DateTime.UtcNow
        };

        _context.ProductComments.Add(comment);
        await _context.SaveChangesAsync();

        commentDto.Id = comment.Id;
        commentDto.CreatedDate = comment.CreatedDate;

        return CreatedAtAction(nameof(GetComments), new { id = id }, commentDto);
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
        
        var directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/{fileName}";
    }

    private static ProductDto MapToDto(Product p) => new ProductDto
    {
        Id = p.Id.ToString(),
        Name = p.Name,
        Category = p.Category,
        Description = p.Description,
        ImageUrl = p.ImageUrl,
        ImageUrls = p.ImageUrls,
        Price = p.Price,
        StockQuantity = p.StockQuantity,
        Features = p.Features,
        Rating = p.Rating,
        ReviewCount = p.ReviewCount
    };

    private async Task PublishProductCreated(Product product)
    {
        try { await _eventBus.PublishAsync(new Common.Events.ProductCreatedIntegrationEvent(product.Id.ToString(), product.Name, product.StockQuantity)); }
        catch (Exception ex) { _logger.LogError(ex, "Publish Error"); }
    }

    private async Task PublishProductDeleted(string id)
    {
        try { await _eventBus.PublishAsync(new Common.Events.ProductDeletedIntegrationEvent(id)); }
        catch (Exception ex) { _logger.LogError(ex, "Publish Error"); }
    }

    private async Task PublishProductUpdated(Product product)
    {
        try { await _eventBus.PublishAsync(new Common.Events.ProductUpdatedIntegrationEvent(product.Id.ToString(), product.Name)); }
        catch (Exception ex) { _logger.LogError(ex, "Publish Error"); }
    }
}