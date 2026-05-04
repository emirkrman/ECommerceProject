using ECommerceProject.Business.Models.Common;
using ECommerceProject.Business.Models.Products;
using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Data.Repositories.Abstract;
using ECommerceProject.Data.UnitOfWork;
using ECommerceProject.Entity.Concrete;
using Microsoft.AspNetCore.Http;

namespace ECommerceProject.Business.Services.Concrete;

public class ProductService : IProductService
{
    private const int ProductListPageSize = 8;
    private const int HomePageSize = 12;

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IImageService _imageService;
    private readonly IStockReservationService _stockReservationService;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IImageService imageService,
        IStockReservationService stockReservationService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _imageService = imageService;
        _stockReservationService = stockReservationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductListResult> GetPublicListAsync(int? categoryId, string? search, string? sort, int page)
    {
        page = Math.Max(page, 1);
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var categoryIds = categoryId.HasValue
            ? await _categoryRepository.GetCategoryAndSubCategoryIdsAsync(categoryId.Value)
            : null;

        var totalProducts = await _productRepository.CountPublicListAsync(categoryIds, search);
        var products = await _productRepository.GetPublicListAsync(
            categoryIds,
            search,
            sort,
            (page - 1) * ProductListPageSize,
            ProductListPageSize);

        return new ProductListResult
        {
            Products = products,
            Categories = await _categoryRepository.GetNavigationCategoriesAsync(),
            CategoryId = categoryId,
            Search = search,
            Sort = sort,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalProducts / (double)ProductListPageSize)
        };
    }

    public async Task<ProductDetailsResult?> GetPublicDetailsAsync(int id)
    {
        var product = await _productRepository.GetActiveDetailsAsync(id);
        if (product == null)
            return null;

        return new ProductDetailsResult
        {
            Product = product,
            AvailableStock = await _stockReservationService.GetAvailableStockAsync(product.Id, product.Stock)
        };
    }

    public async Task<ProductListResult> GetHomeProductsAsync(int page)
    {
        page = Math.Max(page, 1);

        var totalProducts = await _productRepository.CountPublicListAsync(null, null);
        var totalPages = (int)Math.Ceiling(totalProducts / (double)HomePageSize);
        if (totalPages > 0)
            page = Math.Min(page, totalPages);

        var products = await _productRepository.GetPublicListAsync(
            null,
            null,
            "newest",
            (page - 1) * HomePageSize,
            HomePageSize);

        return new ProductListResult
        {
            Products = products,
            CurrentPage = page,
            TotalPages = totalPages
        };
    }

    public async Task<List<Product>> GetAdminListAsync(string? sort)
    {
        return await _productRepository.GetAdminListAsync(sort);
    }

    public async Task<List<Category>> GetActiveCategoriesAsync()
    {
        return await _categoryRepository.GetActiveAsync();
    }

    public async Task<ProductFormData?> GetEditFormAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product == null ? null : MapToFormData(product);
    }

    public async Task<Product?> GetForDeleteAsync(int id)
    {
        return await _productRepository.GetByIdWithCategoryAsync(id);
    }

    public IReadOnlyList<ServiceValidationError> ValidateImage(IFormFile? imageFile, string fieldName)
    {
        return _imageService.ValidateProductImage(imageFile, fieldName);
    }

    public async Task CreateAsync(ProductFormData formData)
    {
        var imageUrl = await _imageService.SaveProductImageAsync(formData.ImageFile);
        var entity = CreateProductEntity(formData, imageUrl);

        try
        {
            await _productRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            _imageService.DeleteProductImage(imageUrl);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, ProductFormData formData)
    {
        var entity = await _productRepository.GetByIdAsync(id);
        if (entity == null)
            return false;

        ApplyFormValues(entity, formData);

        string? oldImageUrl = null;
        string? newImageUrl = null;
        if (formData.ImageFile != null)
        {
            oldImageUrl = entity.ImageUrl;
            newImageUrl = await _imageService.SaveProductImageAsync(formData.ImageFile);
            entity.ImageUrl = newImageUrl;
        }

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            _imageService.DeleteProductImage(newImageUrl);
            throw;
        }

        _imageService.DeleteProductImage(oldImageUrl);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return false;

        var imageUrl = product.ImageUrl;
        _productRepository.Remove(product);
        await _unitOfWork.SaveChangesAsync();
        _imageService.DeleteProductImage(imageUrl);
        return true;
    }

    private static decimal GenerateInitialRating()
    {
        return Random.Shared.Next(35, 51) / 10m;
    }

    private static Product CreateProductEntity(ProductFormData formData, string? imageUrl)
    {
        return new Product
        {
            Name = formData.Name,
            Description = formData.Description,
            Price = formData.Price,
            Stock = formData.Stock,
            ImageUrl = imageUrl,
            CategoryId = formData.CategoryId,
            IsActive = formData.IsActive,
            CreatedDate = DateTime.UtcNow,
            Rating = GenerateInitialRating()
        };
    }

    private static ProductFormData MapToFormData(Product product)
    {
        return new ProductFormData
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ExistingImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive
        };
    }

    private static void ApplyFormValues(Product entity, ProductFormData formData)
    {
        entity.Name = formData.Name;
        entity.Description = formData.Description;
        entity.Price = formData.Price;
        entity.Stock = formData.Stock;
        entity.IsActive = formData.IsActive;
        entity.CategoryId = formData.CategoryId;
        entity.UpdatedDate = DateTime.UtcNow;
    }
}
