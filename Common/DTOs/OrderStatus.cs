namespace Common.DTOs;

public enum OrderStatus
{
    Pending = 0,      // Beklemede
    Processing = 1,   // Hazırlanıyor
    Shipped = 2,      // Kargoya Verildi
    Completed = 3,    // Tamamlandı
    Cancelled = 4     // İptal Edildi
}
