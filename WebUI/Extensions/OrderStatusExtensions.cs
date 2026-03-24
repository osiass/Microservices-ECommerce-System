using Common.DTOs;

namespace WebUI.Extensions;

public static class OrderStatusExtensions
{
    public static string ToStatusText(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Beklemede",
        OrderStatus.Processing => "Hazırlanıyor",
        OrderStatus.Shipped => "Kargoda",
        OrderStatus.Completed => "Tamamlandı",
        OrderStatus.Cancelled => "İptal Edildi",
        _ => status.ToString()
    };

    public static string ToStatusClass(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "bg-warning text-dark",
        OrderStatus.Processing => "bg-info text-white",
        OrderStatus.Shipped => "bg-primary text-white",
        OrderStatus.Completed => "bg-success text-white",
        OrderStatus.Cancelled => "bg-danger text-white",
        _ => "bg-secondary text-white"
    };
}
