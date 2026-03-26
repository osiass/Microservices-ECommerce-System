namespace Common.DTOs;

public class OrderStatusDto
{
    public OrderStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int StatusCode => (int)Status;
}
