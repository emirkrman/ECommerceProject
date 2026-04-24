using ECommerceProject.Entity.Common;

namespace ECommerceProject.Entity.Concrete;

public class Product : BaseEntity
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}