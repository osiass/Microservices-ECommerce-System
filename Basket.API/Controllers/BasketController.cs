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
    [Authorize]
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
            var basket = await _context.ShoppingCarts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.UserName == userName);

            if (basket == null)
            {
                return Ok(new ShoppingCart { UserName = userName });
            }
            return Ok(basket);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateBasket([FromBody] ShoppingCart shoppingCart)
        {
            var catalogClient = _httpClientFactory.CreateClient("Catalog");
            var discountClient = _httpClientFactory.CreateClient("Discount");
            
            // OPTIMIZATION: Fetch all products once instead of inside the loop (N+1 fix)
            var catalogProducts = await catalogClient.GetFromJsonAsync<List<ProductDto>>("api/products") ?? new List<ProductDto>();

            var existingBasket = await _context.ShoppingCarts.FirstOrDefaultAsync(x => x.UserName == shoppingCart.UserName);

            foreach (var item in shoppingCart.Items)
            {
                var realProduct = catalogProducts.FirstOrDefault(p => p.Name == item.ProductName);

                if (realProduct != null)
                {
                    // Discounttan indirimi sor
                    var discount = await discountClient.GetFromJsonAsync<CouponDto>($"api/discount/{item.ProductName}");

                    // Gerçek fiyatı set et (Catalog fiyatı - İndirim miktarı)
                    item.Price = realProduct.Price - (discount?.Amount ?? 0);
                }
            }

            if (existingBasket != null)
            {
                _context.ShoppingCarts.Remove(existingBasket);
                await _context.SaveChangesAsync();
            }
            _context.ShoppingCarts.Add(shoppingCart);
            await _context.SaveChangesAsync();
            return Ok(shoppingCart);

        }

        [HttpDelete]
        public async Task<ActionResult> DeleteBasket(string userName)
        {
            var existingBasket = await _context.ShoppingCarts.FirstOrDefaultAsync(x => x.UserName == userName);
            if (existingBasket == null)
            {
                return NotFound();
            }
            _context.ShoppingCarts.Remove(existingBasket);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
