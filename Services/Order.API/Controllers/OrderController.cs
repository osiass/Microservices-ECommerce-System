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
        private OrderContext _context;
        public OrderController(OrderContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("test-auth")]
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
        //actionresult http cevabı dönücek,Ienurable bir veri değil liste dolusu veri göndericem,orderentity herbir
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(string userName)
        {
            var orders = await _context.Orders
                .Include(x => x.OrderItems)
                .Where(o => o.UserName == userName)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                UserName = o.UserName,
                AddressLine = o.AddressLine,
                TotalPrice = o.TotalPrice,
                CreatedDate = o.CreatedDate,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    Price = oi.Price,
                    Quantity = oi.Quantity
                }).ToList()
            }).ToList();

            return Ok(orderDtos);
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var order = await _context.Orders
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserName = order.UserName,
                AddressLine = order.AddressLine,
                TotalPrice = order.TotalPrice,
                CreatedDate = order.CreatedDate,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    Price = oi.Price,
                    Quantity = oi.Quantity
                }).ToList()
            };

            return Ok(orderDto);
        }

        [HttpPost("checkout")]
        public async Task<ActionResult> Checkout([FromBody] BasketCheckoutDto checkoutData, [FromServices] IHttpClientFactory httpClientFactory, [FromServices] IServiceProvider serviceProvider)
        {
            // EventBusı güvenli bir şekilde al 
            IEventBus? eventBus = null;
            try {
                eventBus = serviceProvider.GetService<IEventBus>();
            } catch (Exception ex) {
                Console.WriteLine($"[Order.API] EventBus servis hatası: {ex.Message}");
            }
            Console.WriteLine($"[Order.API] Checkout başladı. User: {checkoutData.UserName}, Adres: {checkoutData.AddressLine}");
            
            // PaymentAPI için Client oluştur
            var client = httpClientFactory.CreateClient("payment-api");

            // Ödeme isteği hazırla
            var paymentRequest = new PaymentRequestDto
            {
                CardNumber = checkoutData.CardNumber,
                CardHolderName = checkoutData.CardHolderName,
                ExpirationMonth = checkoutData.ExpirationMonth,
                ExpirationYear = checkoutData.ExpirationYear,
                CVV = checkoutData.CVV,
                Amount = checkoutData.Items.Sum(x => x.Price * x.Quantity)
            };

            // Payment.API'ye istek at (10 saniye timeout)
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
                Console.WriteLine("[Order.API] Ödeme başarıyla tamamlandı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Order.API] Payment API hatası: {ex.Message}.");
                return BadRequest($"Ödeme servisine ulaşılamadı: {ex.Message}");
            }

            // Siparişi veritabanına kaydet
            Console.WriteLine("[Order.API] Sipariş veritabanına kaydediliyor...");
            var newOrder = new Order.API.Entities.Order
            {
                UserName = checkoutData.UserName,
                AddressLine = checkoutData.AddressLine,
                TotalPrice = paymentRequest.Amount,
                CreatedDate = DateTime.UtcNow,
                OrderItems = checkoutData.Items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                }).ToList()
            };

            try 
            {
                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[Order.API] Sipariş kaydedildi. OrderId: {newOrder.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Order.API] VERİTABANI HATASI: {ex.Message}");
                return StatusCode(500, $"Veritabanı hatası: {ex.Message}");
            }

            // EventBus publish — RabbitMQ yoksa checkout'u bloklama (3 saniye timeout)
            Console.WriteLine("[Order.API] EventBus publish deneniyor...");
            if (eventBus != null)
            {
                try
                {
                    var orderCreatedEvent = new OrderCreatedIntegrationEvent
                    {
                        OrderId = newOrder.Id,
                        UserName = newOrder.UserName,
                        Items = newOrder.OrderItems.Select(x => new OrderItemStockData
                        {
                            ProductId = x.ProductId,
                            Quantity = x.Quantity
                        }).ToList()
                    };

                    var publishTask = eventBus.PublishAsync(orderCreatedEvent);
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(3));

                    // Hangisi önce biterse onu al — RabbitMQ 3 saniye içinde cevap vermezse devam et
                    var finishedTask = await Task.WhenAny(publishTask, timeoutTask);
                    if (finishedTask == timeoutTask)
                    {
                        Console.WriteLine("[Order.API] EventBus zaman aşımına uğradı, devam ediliyor.");
                    }
                    else
                    {
                        Console.WriteLine("[Order.API] EventBus başarıyla publish edildi.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Order.API] EventBus hatası: {ex.Message}. Sipariş yine de onaylandı.");
                }
            }
            else
            {
                Console.WriteLine("[Order.API] EventBus servis bağlantısı yok, publish atlanıyor.");
            }

            Console.WriteLine("[Order.API] Checkout başarıyla bitti.");
            return Ok(new { message = "Ödeme onaylandı ve siparişiniz alındı.", orderId = newOrder.Id });
        }
    }
}
