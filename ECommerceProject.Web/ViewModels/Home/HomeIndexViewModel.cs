using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Web.ViewModels.Home;

public class HomeIndexViewModel
{
    public List<Product> Products { get; set; } = new();

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }
}
