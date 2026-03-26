using System;

namespace Payment.API.Entities
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Success";
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string TransactionId { get; set; } = string.Empty;
    }
}
