using Microsoft.EntityFrameworkCore;
using Order.API.Entities;
namespace Order.API.Data
{
    public class OrderContext:DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options) : base(options) { } 
        public DbSet<Order.API.Entities.Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
