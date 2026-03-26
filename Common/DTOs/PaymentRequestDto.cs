using System;

namespace Common.DTOs
{
    // Ödeme işlemi için istemciden Order.API vb.alınması gereken kredi kartı ve sepet verileri.
    public class PaymentRequestDto
    {

        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public string CVV { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
