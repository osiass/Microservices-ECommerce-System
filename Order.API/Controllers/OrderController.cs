using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Order.API.DTOs;
using Order.API.Entities;

namespace Order.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private OrderContext _context;
        public OrderController(OrderContext context)
        {
            _context = context;
        }

        [HttpGet("{userName")]
        //actionresult http cevabı dönücek,Ienurable bir veri değil liste dolusu veri göndericem,orderentity herbir parça sipariş nesnesi
        public async Task<ActionResult<IEnumerable<Order.API.Entities.Order>>> GetOrders(string userName)
        {
            var orders = await _context.Orders
                .Include(x=>x.OrderItems)
                .Where(o => o.UserName == userName)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpPost("checkout")]
        public async Task<ActionResult> Checkout([FromBody] BasketCheckoutDto checkoutData)
        {
            //gelen dtoyu gerçek order nesneesine döndür
            var newOrder = new Order.API.Entities.Order
            {
                UserName = checkoutData.UserName,
                AddressLine = checkoutData.AddressLine,
                TotalPrice = checkoutData.Items.Sum(x => x.Price * x.Quantity),
                CreatedDate = DateTime.UtcNow,
                OrderItems = checkoutData.Items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                }).ToList()
            };
            //sıfırlıyoz
            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            return Ok(new { message = "siparişiniz alındı", orderId = newOrder.Id });
        }
    }
}
