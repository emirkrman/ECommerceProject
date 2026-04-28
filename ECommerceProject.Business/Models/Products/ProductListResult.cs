using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Business.Models.Products;

public class ProductListResult
{
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public int? CategoryId { get; set; }
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
