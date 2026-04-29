using ECommerceProject.Entity.Common;

namespace ECommerceProject.Entity.Concrete;

public class Cart : BaseEntity
{
    public int UserId { get; set; }

    public AppUser? User { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; } = 1;
}
