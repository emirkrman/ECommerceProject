using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Web.ViewModels.Products;

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = null!;

    public int AvailableStock { get; set; }
}
