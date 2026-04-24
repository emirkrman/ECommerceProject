using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Web.ViewModels.Home;

public class HomeIndexViewModel
{
    public List<Product> PopularProducts { get; set; } = new();

    public List<Category> Categories { get; set; } = new();
}