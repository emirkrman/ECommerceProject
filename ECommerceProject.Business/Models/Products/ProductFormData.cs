using Microsoft.AspNetCore.Http;

namespace ECommerceProject.Business.Models.Products;

public class ProductFormData
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ExistingImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public bool IsActive { get; set; } = true;
    public int CategoryId { get; set; }
}
