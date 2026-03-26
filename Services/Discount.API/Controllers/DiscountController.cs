using Discount.API.Data;
using Discount.API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CouponDto>))]
        public async Task<ActionResult<IEnumerable<CouponDto>>> GetCoupons()
        {
            var coupons = await _context.Coupons.AsNoTracking().ToListAsync();
            return Ok(coupons.Select(MapToDto).ToList());
        }

        [HttpGet("{code}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CouponDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CouponDto>> GetDiscount(string code)
        {
            var coupon = await _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower() && c.IsActive);
            
            if (coupon == null)
            {
                return NotFound(new { message = "İndirim kodu bulunamadı veya geçersiz." });
            }
            return Ok(MapToDto(coupon));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CouponDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CouponDto>> CreateDiscount([FromBody] CouponDto couponDto)
        {
            var coupon = new Coupon
            {
                ProductName = couponDto.ProductName,
                Code = couponDto.Code,
                Description = couponDto.Description,
                Amount = couponDto.Amount,
                IsActive = couponDto.IsActive
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetDiscount), new { code = coupon.Code }, MapToDto(coupon));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDiscount(int id, [FromBody] CouponDto couponDto)
        {
            if (id != couponDto.Id) return BadRequest();

            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

            coupon.ProductName = couponDto.ProductName;
            coupon.Code = couponDto.Code;
            coupon.Description = couponDto.Description;
            coupon.Amount = couponDto.Amount;
            coupon.IsActive = couponDto.IsActive;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static CouponDto MapToDto(Coupon c) => new CouponDto
        {
            Id = c.Id,
            ProductName = c.ProductName,
            Code = c.Code,
            Description = c.Description,
            Amount = c.Amount,
            IsActive = c.IsActive
        };
    }
}
