using Common.Entities;

namespace Catalog.API.Entities;

public class ProductComment : BaseEntity
{
    public Guid ProductId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
