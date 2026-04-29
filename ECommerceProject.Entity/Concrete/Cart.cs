using ECommerceProject.Entity.Common;

namespace ECommerceProject.Entity.Concrete;

public class Cart : BaseEntity
{
    public int UserId { get; set; }

    public AppUser? User { get; set; }

    public List<CartItem> Items { get; set; } = new();
}
