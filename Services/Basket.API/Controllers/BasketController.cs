using Basket.API.Data;
using Common.DTOs;
using Basket.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly BasketContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BasketController> _logger;
        
        public BasketController(BasketContext context, IHttpClientFactory httpClientFactory, ILogger<BasketController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShoppingCart))]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var cleanedUserName = userName?.Trim().ToLower() ?? string.Empty;
            
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShoppingCart))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart shoppingCart)
        {
            shoppingCart.UserName = shoppingCart.UserName?.Trim().ToLower() ?? string.Empty;
            
            // Katalog ve İndirim bilgilerini tazele
            await EnrichBasketItems(shoppingCart.Items);

            var existingBasket = await _context.ShoppingCarts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.UserName == shoppingCart.UserName);

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

        [HttpPost("add-item")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShoppingCart))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShoppingCart>> AddItem([FromBody] AddItemRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserName))
                return BadRequest("Invalid request data.");

            var userName = request.UserName.ToLower();
            
            var basket = await _context.ShoppingCarts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.UserName == userName);

            if (basket == null)
            {
                basket = new ShoppingCart { UserName = userName, Items = new List<BasketItem>() };
                _context.ShoppingCarts.Add(basket);
            }

            _logger.LogInformation("[Basket.API] GetDiscountAmount started for {ProductName}", request.ProductName);
            // İndirim miktarını al
            var discountAmount = await GetDiscountAmount(request.ProductName);
            _logger.LogInformation("[Basket.API] GetDiscountAmount finished. Amount: {Amount}", discountAmount);
            var finalPrice = request.Price - discountAmount;
            
            var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                existingItem.ProductName = request.ProductName;
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
                    Quantity = request.Quantity,
                    ImageUrl = request.ImageUrl
                });
            }

            await _context.SaveChangesAsync();
            return Ok(basket);
        }

        [HttpDelete("{userName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteBasket(string userName)
        {
            var cleanedUserName = userName?.Trim().ToLower() ?? string.Empty;
            
            var existingBasket = await _context.ShoppingCarts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.UserName == cleanedUserName);
                
            if (existingBasket == null)
                return Ok(new { message = "Basket not found." }); 

            if (existingBasket.Items != null)
                _context.RemoveRange(existingBasket.Items);
            
            _context.ShoppingCarts.Remove(existingBasket);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Basket deleted successfully." });
        }

        private async Task EnrichBasketItems(List<BasketItem> items)
        {
            var catalogClient = _httpClientFactory.CreateClient("Catalog");
            // Katalogdan ürünleri getirirken async/await ve güvenli client kullanımı
            try {
                var catalogProducts = await catalogClient.GetFromJsonAsync<List<ProductDto>>("api/products") ?? new();
                foreach (var item in items)
                {
                    var realProduct = catalogProducts.FirstOrDefault(p => p.Name == item.ProductName);
                    if (realProduct != null)
                    {
                        var discount = await GetDiscountAmount(item.ProductName);
                        item.Price = realProduct.Price - discount;
                        item.OriginalPrice = realProduct.Price;
                        item.ImageUrl = realProduct.ImageUrl;
                    }
                }
            } catch { /* Catalog servis hatası basket işlemini tamamen bozmamalı */ }
        }

        private async Task<decimal> GetDiscountAmount(string productName)
        {
            try
            {
                var discountClient = _httpClientFactory.CreateClient("Discount");
                var discount = await discountClient.GetFromJsonAsync<CouponDto>($"api/discount/{productName}");
                return discount?.Amount ?? 0;
            }
            catch { return 0; }
        }
    }
}
