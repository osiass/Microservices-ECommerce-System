using Inventory.API.Data;
using Inventory.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<StockDto>))]
    public async Task<ActionResult<IEnumerable<StockDto>>> GetStocks()
    {
        var stocks = await _context.Stocks.AsNoTracking().ToListAsync();
        return Ok(stocks.Select(s => new StockDto 
        { 
            ProductId = s.ProductId,
            ProductName = s.ProductName,
            Quantity = s.Quantity 
        }).ToList());
    }

}
