public class OrderReportDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<ProductSalesDto> Products { get; set; } = new();
}

public class ProductSalesDto
{
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}