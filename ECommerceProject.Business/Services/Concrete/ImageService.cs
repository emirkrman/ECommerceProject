using ECommerceProject.Business.Models.Common;
using ECommerceProject.Business.Services.Abstract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ECommerceProject.Business.Services.Concrete;

public class ImageService : IImageService
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedImageContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxImageSizeBytes = 2 * 1024 * 1024;
    private const int MaxImageWidth = 1200;
    private const int MaxImageHeight = 1200;
    private const int WebpQuality = 80;

    private readonly IWebHostEnvironment _environment;

    public ImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public IReadOnlyList<ServiceValidationError> ValidateProductImage(IFormFile? imageFile, string fieldName)
    {
        var errors = new List<ServiceValidationError>();

        if (imageFile == null || imageFile.Length == 0)
            return errors;

        var extension = Path.GetExtension(imageFile.FileName);
        if (string.IsNullOrWhiteSpace(extension) ||
            !AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add(new ServiceValidationError(fieldName, "Lütfen JPG, PNG veya WEBP formatında bir görsel seçin."));
        }

        if (!AllowedImageContentTypes.Contains(imageFile.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add(new ServiceValidationError(fieldName, "Yüklenen dosya geçerli bir görsel olmalı."));
        }

        if (imageFile.Length > MaxImageSizeBytes)
        {
            errors.Add(new ServiceValidationError(fieldName, "Görsel boyutu 2 MB'dan büyük olamaz."));
        }

        return errors;
    }

    public async Task<string?> SaveProductImageAsync(IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return null;

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}.webp";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var inputStream = imageFile.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(MaxImageWidth, MaxImageHeight)
        }));

        await using var outputStream = new FileStream(filePath, FileMode.Create);
        await image.SaveAsync(outputStream, new WebpEncoder
        {
            Quality = WebpQuality
        });

        return $"/uploads/products/{fileName}";
    }

    public void DeleteProductImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) ||
            !imageUrl.StartsWith("/uploads/products/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
