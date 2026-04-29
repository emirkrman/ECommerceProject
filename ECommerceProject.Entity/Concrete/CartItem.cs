using ECommerceProject.Entity.Common;

namespace ECommerceProject.Entity.Concrete;

public class CartItem : BaseEntity
{
    public int CartId { get; set; }

    public Cart? Cart { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; } = 1;
}
