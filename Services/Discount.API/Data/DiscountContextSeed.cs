using Discount.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Discount.API.Data;

public static class DiscountContextSeed
{
    public static async Task SeedAsync(DiscountContext context)
    {
        // Eğer tabloda hiç kupon yoksa, bu örnek kuponları ekle
        if (!await context.Coupons.AnyAsync())
        {
            context.Coupons.AddRange(new List<Coupon>
            {
                new Coupon { ProductName = "Gaming Mouse", Amount = 100 },
                new Coupon { ProductName = "Monitor", Amount = 500 }
            });

            await context.SaveChangesAsync();
        }
    }
}