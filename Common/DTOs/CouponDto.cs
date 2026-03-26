namespace Common.DTOs;

public class CouponDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Amount { get; set; }
    public bool IsActive { get; set; } = true;
}
