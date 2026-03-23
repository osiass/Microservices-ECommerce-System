using Common.Entities;

namespace Catalog.API.Entities;
//mikroserviste id için guid commondan geliyor onu siliyoruz datetimeda aynı şekilde

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

