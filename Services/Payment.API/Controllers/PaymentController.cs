using Microsoft.AspNetCore.Mvc;
using Common.DTOs;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http.HttpResults;

//Order.API'den gelecek ödeme HTTP isteklerini bu Controller üzerinden karşılayacak.
namespace Payment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto paymentDto)
        {
            if (paymentDto.CardNumber.StartsWith("4"))
                return Ok(true);
            return BadRequest("Geçersiz kredi kartı!");
        }
        
    }
}
