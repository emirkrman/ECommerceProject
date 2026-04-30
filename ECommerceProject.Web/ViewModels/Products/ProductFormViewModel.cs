using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ECommerceProject.Web.ViewModels.Products;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün adı boş olamaz.")]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public string? ExistingImageUrl { get; set; }

    public IFormFile? ImageFile { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}
