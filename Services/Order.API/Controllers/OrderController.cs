using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Order.API.Entities;
using System.Net.Http.Json;
using Common.DTOs;
using Common.Events;
using Common.EventBus;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderContext _context;
        private readonly IEventBus? _eventBus;
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderController(OrderContext context, IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            
            try {
                _eventBus = serviceProvider.GetService<IEventBus>();
            } catch (Exception ex) {
                Console.WriteLine($"[Order.API] EventBus servis hatası: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpGet("test-auth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> TestAuth()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            bool dbOnline = false;
            try {
                dbOnline = await _context.Database.CanConnectAsync();
            } catch { }

            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated,
                UserName = User.Identity?.Name,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }),
                HasHeader = !string.IsNullOrEmpty(authHeader),
                HeaderValue = authHeader.Length > 20 ? authHeader.Substring(0, 20) + "..." : authHeader,
                DatabaseOnline = dbOnline
            });
        }

        [HttpGet("{userName}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderDto>))]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(string userName)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.OrderItems)
                .Where(o => o.UserName == userName)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            return Ok(orders.Select(MapToOrderDto).ToList());
        }

        [HttpGet("detail/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return Ok(MapToOrderDto(order));
        }

        [HttpGet("check-purchase/{userName}/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<ActionResult<bool>> CheckPurchase(string userName, string productId)
        {
            var cleanedUserName = userName?.Trim().ToLower() ?? string.Empty;
            
            var hasBought = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .AnyAsync(o => o.UserName == cleanedUserName && o.OrderItems.Any(oi => oi.ProductId == productId));

            return Ok(hasBought);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.OrderItems)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            return Ok(orders.Select(MapToOrderDto).ToList());
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto updateDto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = updateDto.Status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("checkout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Checkout([FromBody] BasketCheckoutDto checkoutData)
        {
            Console.WriteLine($"[Order.API] Checkout başladı. User: {checkoutData.UserName}, Adres: {checkoutData.AddressLine}");
            
            // PaymentAPI için Client oluştur
            var client = _httpClientFactory.CreateClient("payment-api");

            // Ödeme isteği hazırla
            var paymentRequest = new PaymentRequestDto
            {
                CardNumber = checkoutData.CardNumber,
                CardHolderName = checkoutData.CardHolderName,
                ExpirationMonth = checkoutData.ExpirationMonth,
                ExpirationYear = checkoutData.ExpirationYear,
                CVV = checkoutData.CVV,
                Amount = checkoutData.Items.Sum(x => x.Price * x.Quantity) - (decimal)checkoutData.Discount
            };

            // Payment.API'ye istek at
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await client.PostAsJsonAsync("api/payment", paymentRequest, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Order.API] Ödeme başarısız! Status: {response.StatusCode}, Detay: {errorMsg}");
                    return BadRequest($"Ödeme işlemi başarısız oldu: {errorMsg}");
                }

                // Ödeme başarılı, TransactionId'yi al
                var paymentResult = await response.Content.ReadFromJsonAsync<PaymentResult>();
                var transactionId = paymentResult?.TransactionId ?? "N/A";
                
                Console.WriteLine($"[Order.API] Ödeme başarıyla tamamlandı. TransId: {transactionId}");

                // Siparişi veritabanına kaydet
                var newOrder = new Entities.Order
                {
                    UserName = checkoutData.UserName,
                    UserEmail = checkoutData.UserEmail,
                    AddressLine = checkoutData.AddressLine,
                    TotalPrice = paymentRequest.Amount,
                    CreatedDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    CouponCode = checkoutData.CouponCode,
                    Discount = (decimal)checkoutData.Discount,
                    TransactionId = transactionId,
                    OrderItems = checkoutData.Items.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Price = item.Price,
                        Quantity = item.Quantity,
                        ImageUrl = item.ImageUrl
                    }).ToList()
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[Order.API] Sipariş kaydedildi. OrderId: {newOrder.Id}");

                // RabbitMQ'ya event fırlat
                if (_eventBus != null)
                {
                    _ = PublishOrderCreatedEvent(newOrder);
                }

                return Ok(new { message = "Ödeme onaylandı ve siparişiniz alındı.", orderId = newOrder.Id, transactionId = transactionId });
            }
            catch (Exception ex)
            {
                var fullError = ex.Message + (ex.InnerException != null ? (" | Inner: " + ex.InnerException.Message) : "");
                if (ex.InnerException?.InnerException != null) 
                    fullError += " | Root: " + ex.InnerException.InnerException.Message;

                Console.WriteLine($"[Order.API] Hata: {fullError}");
                return BadRequest($"İşlem sırasında bir hata oluştu: {fullError}");
            }
        }

        private async Task PublishOrderCreatedEvent(Entities.Order order)
        {
            try
            {
                var orderCreatedEvent = new OrderCreatedIntegrationEvent
                {
                    OrderId = order.Id,
                    UserName = order.UserName,
                    UserEmail = order.UserEmail,
                    TotalPrice = order.TotalPrice,
                    Items = order.OrderItems.Select(x => new OrderItemStockData
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity
                    }).ToList()
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await _eventBus!.PublishAsync(orderCreatedEvent);
                Console.WriteLine("[Order.API] OrderCreated event başarıyla publish edildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Order.API] EventBus hatası: {ex.Message}. Sipariş yine de onaylandı.");
            }
        }

        private static OrderDto MapToOrderDto(Entities.Order o) => new OrderDto
        {
            Id = o.Id,
            UserName = o.UserName,
            AddressLine = o.AddressLine,
            TotalPrice = o.TotalPrice,
            CreatedDate = o.CreatedDate,
            Status = o.Status,
            CouponCode = o.CouponCode,
            Discount = o.Discount,
            TransactionId = o.TransactionId,
            OrderItems = o.OrderItems.Select(MapToOrderItemDto).ToList()
        };

        private static OrderItemDto MapToOrderItemDto(OrderItem oi) => new OrderItemDto
        {
            Id = oi.Id,
            ProductId = oi.ProductId,
            ProductName = oi.ProductName,
            Price = oi.Price,
            Quantity = oi.Quantity,
            ImageUrl = oi.ImageUrl
        };
    }

    public record PaymentResult(bool Success, string TransactionId);
}
