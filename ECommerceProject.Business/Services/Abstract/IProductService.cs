using ECommerceProject.Business.Models.Common;
using ECommerceProject.Business.Models.Products;
using ECommerceProject.Entity.Concrete;
using Microsoft.AspNetCore.Http;

namespace ECommerceProject.Business.Services.Abstract;

public interface IProductService
{
    Task<ProductListResult> GetPublicListAsync(int? categoryId, string? search, string? sort, int page);
    Task<Product?> GetActiveDetailsAsync(int id);
    Task<List<Product>> GetAdminListAsync(string? sort);
    Task<List<Product>> GetLatestActiveProductsAsync(int count);
    Task<List<Category>> GetActiveCategoriesAsync();
    Task<ProductFormData?> GetEditFormAsync(int id);
    Task<Product?> GetForDeleteAsync(int id);
    IReadOnlyList<ServiceValidationError> ValidateImage(IFormFile? imageFile, string fieldName);
    Task CreateAsync(ProductFormData formData);
    Task<bool> UpdateAsync(int id, ProductFormData formData);
    Task<bool> DeleteAsync(int id);
}
