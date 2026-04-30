using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Business.Models.Products;

public class ProductDetailsResult
{
    public Product Product { get; set; } = null!;

    public int AvailableStock { get; set; }
}
