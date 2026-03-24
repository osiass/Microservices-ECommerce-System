using Catalog.API.Data;
using Catalog.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Common.DTOs;

namespace Catalog.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly CatalogContext _context;

    public ProductsController(CatalogContext context)
    {
        _context = context;
    }

    [HttpGet] // URL: api/products?search=
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts(string? search, string? category)
    {
        IQueryable<Product> query = _context.Products;

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        return await query.ToListAsync();
    }

    [HttpGet("{id}")] 
    public async Task<ActionResult<Product>> GetProduct(string id)
    {   
        if(!Guid.TryParse(id, out var guidId)) return BadRequest("Invalid product ID");
        var product = await _context.Products.FindAsync(guidId);
        if (product == null) return NotFound();
        return product;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Product>> DeleteProduct(string id)
    {
        if(!Guid.TryParse(id, out var guidId)) return BadRequest("Invalid product ID");
        var product = await _context.Products.FindAsync(guidId);
        if(product == null) return NotFound();
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(string id, Product product)
    {
        if(id != product.Id.ToString()) return BadRequest("Invalid product ID");
        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/rate")]
    public async Task<ActionResult> RateProduct(string id, [FromBody] int score)
    {
        if(!Guid.TryParse(id, out var guidId)) return BadRequest();
        var product = await _context.Products.FindAsync(guidId);
        if(product == null) return NotFound();

        double totalScore = (product.Rating * product.ReviewCount) + score;
        product.ReviewCount++;
        product.Rating = totalScore / product.ReviewCount;
        
        await _context.SaveChangesAsync();
        return Ok(new RatingDto { Rating = product.Rating, ReviewCount = product.ReviewCount });
    }

    [HttpGet("{id}/comments")]
    public async Task<ActionResult<IEnumerable<ProductComment>>> GetComments(string id)
    {
        if(!Guid.TryParse(id, out var guidId)) return BadRequest();
        return await _context.ProductComments
            .Where(c => c.ProductId == guidId)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<ProductComment>> AddComment(string id, ProductComment comment)
    {
        if(!Guid.TryParse(id, out var guidId)) return BadRequest();
        comment.ProductId = guidId;
        comment.CreatedDate = DateTime.UtcNow;
        _context.ProductComments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }
}