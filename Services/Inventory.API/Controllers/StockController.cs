using Inventory.API.Data;
using Inventory.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly InventoryContext _context;

    public StockController(InventoryContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Stock>>> GetStocks() 
    //koleksiyon ben sana bi stock listesi döndürüyorum ama nasıl olduğu önemli değil list array hashset olablir
    {
        return await _context.Stocks.ToListAsync();
    }

    // Stok Ekleme (Test amaçlı)
    [HttpPost("seed")]
    public async Task<ActionResult> Seed()
    {
        if (await _context.Stocks.AnyAsync()) return BadRequest("Zaten stok var");

        _context.Stocks.AddRange(new List<Stock>
        {
            new Stock { ProductId = "1", Count = 100 },
            new Stock { ProductId = "2", Count = 50 }
        });

        await _context.SaveChangesAsync();
        return Ok("Stoklar eklendi");
    }
}
