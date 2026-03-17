using Basket.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Basket.API.Data
{
    public class BasketContext : DbContext
    {
        public BasketContext(DbContextOptions<BasketContext> options) : base(options) { }

        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    }
}
