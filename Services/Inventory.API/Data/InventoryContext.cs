using Microsoft.EntityFrameworkCore;
using Inventory.API.Entities;

namespace Inventory.API.Data;

public class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options) { }

    public DbSet<Stock> Stocks { get; set; }
}
