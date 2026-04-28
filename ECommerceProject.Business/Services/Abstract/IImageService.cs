using ECommerceProject.Business.Models.Common;
using Microsoft.AspNetCore.Http;

namespace ECommerceProject.Business.Services.Abstract;

public interface IImageService
{
    IReadOnlyList<ServiceValidationError> ValidateProductImage(IFormFile? imageFile, string fieldName);
    Task<string?> SaveProductImageAsync(IFormFile? imageFile);
    void DeleteProductImage(string? imageUrl);
}
