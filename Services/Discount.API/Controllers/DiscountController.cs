using Discount.API.Data;
using Discount.API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discount.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly DiscountContext _context;
        public DiscountController(DiscountContext context)
        {
            _context = context;
        }

        [HttpGet("{productName}")]
        public async Task<ActionResult<Coupon>> GetDiscount(string productName)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.ProductName == productName);
            
            if (coupon == null)
            {
                return new Coupon
                {
                    ProductName = productName,
                    Amount = 0
                };
            }
            return Ok(coupon);
        }
    }
}
