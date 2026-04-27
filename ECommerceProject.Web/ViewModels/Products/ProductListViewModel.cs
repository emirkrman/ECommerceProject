using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Web.ViewModels.Products;

public class ProductListViewModel
{
    public List<ECommerceProject.Entity.Concrete.Product> ListedProducts { get; set; } = new();

    public List<Category> Categories { get; set; } = new();

    public int? CategoryId { get; set; }

    public string? Search { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }
}