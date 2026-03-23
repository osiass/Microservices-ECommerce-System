using Basket.API.Data;
using Common.DTOs;
using Basket.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Basket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly BasketContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        public BasketController(BasketContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<ActionResult> GetBasket(string userName)
        {
            var cleanedUserName = userName?.Trim().ToLower() ?? string.Empty;
            Console.WriteLine($"[Basket.API] GetBasket: {cleanedUserName}");
            
            var basket = await _context.ShoppingCarts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.UserName == cleanedUserName);

            if (basket == null)
            {
                return Ok(new ShoppingCart { UserName = userName });
            }
            return Ok(basket);
        }
        [HttpPost]
        public async Task<ActionResult> UpdateBasket([FromBody] ShoppingCart shoppingCart)
        {
            try
            {
                var catalogClient = _httpClientFactory.CreateClient("Catalog");
                var discountClient = _httpClientFactory.CreateClient("Discount");         
                var catalogProducts = await catalogClient.GetFromJsonAsync<List<ProductDto>>("api/products") ?? new List<ProductDto>();

                shoppingCart.UserName = shoppingCart.UserName?.Trim().ToLower() ?? string.Empty;
                var existingBasket = await _context.ShoppingCarts
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.UserName == shoppingCart.UserName);

                foreach (var item in shoppingCart.Items)
                {
                    var realProduct = catalogProducts.FirstOrDefault(p => p.Name == item.ProductName);

                    if (realProduct != null)
                    {
                        try
                        {
                            // Discounttan indirimi sor
                            var discount = await discountClient.GetFromJsonAsync<CouponDto>($"api/discount/{item.ProductName}");
                            item.Price = realProduct.Price - (discount?.Amount ?? 0);
                        }
                        catch
                        {
                            item.Price = realProduct.Price;
                        }
                        item.OriginalPrice = realProduct.Price;
                    }
                }

                if (existingBasket != null)
                {
                    _context.RemoveRange(existingBasket.Items);
                    existingBasket.Items = shoppingCart.Items;
                    _context.ShoppingCarts.Update(existingBasket);
                }
                else
                {
                    _context.ShoppingCarts.Add(shoppingCart);
                }

                await _context.SaveChangesAsync();
                return Ok(shoppingCart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("add-item")]
        public async Task<ActionResult> AddItem([FromBody] AddItemRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserName))
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var userName = request.UserName.ToLower();
                var discountClient = _httpClientFactory.CreateClient("Discount");

                var basket = await _context.ShoppingCarts
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.UserName == userName);

                if (basket == null)
                {
                    basket = new ShoppingCart { UserName = userName, Items = new List<BasketItem>() };
                    _context.ShoppingCarts.Add(basket);
                }

                decimal discountAmount = 0;
                try
                {
                    var discount = await discountClient.GetFromJsonAsync<CouponDto>($"api/discount/{request.ProductName}");
                    discountAmount = discount?.Amount ?? 0;
                }
                catch { /* Ignore discount failure */ }

                var finalPrice = request.Price - discountAmount;
                var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity += request.Quantity;
                    existingItem.ProductName = request.ProductName; // Update name in case it changed
                    existingItem.Price = finalPrice; 
                    existingItem.OriginalPrice = request.Price;
                }
                else
                {
                    basket.Items.Add(new BasketItem
                    {
                        ProductId = request.ProductId,
                        ProductName = request.ProductName,
                        Price = finalPrice,
                        OriginalPrice = request.Price,
                        Quantity = request.Quantity
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(basket);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error: {ex.Message}";
                if (ex.InnerException != null) errorMsg += $" | Inner: {ex.InnerException.Message}";
                Console.WriteLine(errorMsg);
                return StatusCode(500, errorMsg);
            }
        }

        [HttpDelete("{userName}")]
        public async Task<ActionResult> DeleteBasket(string userName)
        {
            var cleanedUserName = userName?.Trim().ToLower() ?? string.Empty;
            
            try 
            {
                var existingBasket = await _context.ShoppingCarts
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.UserName == cleanedUserName);
                    
                if (existingBasket == null)
                {
                    Console.WriteLine($"[Basket.API] Sepet bulunamadı, silinecek bir şey yok: '{cleanedUserName}'");
                    return Ok(new { message = "Basket not found, nothing to delete." }); 
                }
                
                if (existingBasket.Items != null)
                {
                    Console.WriteLine($"[Basket.API] {existingBasket.Items.Count} adet ürün temizleniyor...");
                    _context.RemoveRange(existingBasket.Items);
                }
                
                _context.ShoppingCarts.Remove(existingBasket);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"[Basket.API] Sepet TAMAMEN TEMİZLENDİ: '{cleanedUserName}'");
                return Ok(new { message = "Basket deleted successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Basket.API] SİLME HATASI: {ex.Message}");
                return StatusCode(500, $"Delete error: {ex.Message}");
            }
        }
    }
}
