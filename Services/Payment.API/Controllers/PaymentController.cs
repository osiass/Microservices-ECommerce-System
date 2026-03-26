using Microsoft.AspNetCore.Mvc;
using Common.DTOs;
using System.Threading.Tasks;
using Payment.API.Data;
using Payment.API.Entities;

// Order.API'den gelecek ödeme HTTP isteklerini bu Controller üzerinden karşılayacak.
namespace Payment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentContext _context;
        public PaymentController(PaymentContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto paymentDto)
        {
            //Kart numarası uzunluğu 
            if (string.IsNullOrEmpty(paymentDto.CardNumber) || paymentDto.CardNumber.Length < 13)
                return BadRequest("Geçersiz kart numarası.");

            // Son kullanma tarihi kontrolü
            var now = DateTime.Now;
            if (paymentDto.ExpirationYear < now.Year % 100 || 
               (paymentDto.ExpirationYear == now.Year % 100 && paymentDto.ExpirationMonth < now.Month))
            {
                return BadRequest("Kartın son kullanma tarihi geçmiş.");
            }

            // CVV kontrolü
            if (string.IsNullOrEmpty(paymentDto.CVV) || paymentDto.CVV.Length < 3 || paymentDto.CVV.Length > 4 || !paymentDto.CVV.All(char.IsDigit))
                return BadRequest("Geçersiz CVV.");

            // Yapay gecikme 
            await Task.Delay(80);

            var transactionId = Guid.NewGuid().ToString();

            // İŞLEMİ VERİTABANINA KAYDET
            var transaction = new PaymentTransaction
            {
                CardNumber = paymentDto.CardNumber.Substring(0, 4) + "****" + paymentDto.CardNumber.Substring(paymentDto.CardNumber.Length - 4),
                CardHolderName = paymentDto.CardHolderName,
                Amount = paymentDto.Amount,
                Status = "Success",
                TransactionId = transactionId,
                TransactionDate = DateTime.UtcNow
            };

            try 
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"[Payment.API] Ödeme başarıyla kaydedildi. TransId: {transactionId}");
                return Ok(new { Success = true, TransactionId = transactionId });
            }
            catch (Exception ex)
            {
                var fullError = ex.Message + (ex.InnerException != null ? (" | Inner: " + ex.InnerException.Message) : "");
                if (ex.InnerException?.InnerException != null) 
                    fullError += " | Root: " + ex.InnerException.InnerException.Message;

                Console.WriteLine($"[Payment.API] Ödeme kaydedilirken HATA: {fullError}");
                return StatusCode(500, new { type = "Server Error", title = "Sunucu Hatası", detail = fullError });
            }
        }
    }
}
