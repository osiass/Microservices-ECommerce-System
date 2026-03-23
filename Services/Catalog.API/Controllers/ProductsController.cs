using Catalog.API.Data;
using Catalog.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        // IQueryable sorgu veritabanına gitmeden önce burada inşa edilir.
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
}