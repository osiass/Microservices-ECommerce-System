using System.ComponentModel.DataAnnotations;

namespace Common.DTOs;

public class BasketCheckoutDto
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Adres bilgisi zorunludur.")]
    public string AddressLine { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Kart numarası zorunludur.")]
    public string CardNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Kart üzerindeki isim zorunludur.")]
    public string CardHolderName { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 12, ErrorMessage = "Ay 1-12 arasında olmalıdır.")]
    public int ExpirationMonth { get; set; }
    
    [Required]
    public int ExpirationYear { get; set; }
    
    [Required(ErrorMessage = "CVV zorunludur.")]
    [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVV 3 veya 4 haneli olmalıdır.")]
    public string CVV { get; set; } = string.Empty;
    
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public List<BasketItemDto> Items { get; set; } = new();
}