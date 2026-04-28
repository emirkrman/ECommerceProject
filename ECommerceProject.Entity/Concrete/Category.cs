using ECommerceProject.Entity.Common;
using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Entity.Concrete;

public class Category : BaseEntity
{
    [Required(ErrorMessage = "Kategori adi bos olamaz.")]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int? ParentCategoryId { get; set; }

    public Category? ParentCategory { get; set; }

    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
