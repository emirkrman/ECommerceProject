using ECommerceProject.Data.Context;
using ECommerceProject.Entity.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ECommerceProject.Entity.Concrete;
using ECommerceProject.Web.ViewModels.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ECommerceProject.Web.Controllers;

public class ProductController : BaseController
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedImageContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxImageSizeBytes = 2 * 1024 * 1024;
    private const int MaxImageWidth = 1200;
    private const int MaxImageHeight = 1200;
    private const int WebpQuality = 80;
    private readonly IWebHostEnvironment _environment;

    public ProductController(AppDbContext context, IWebHostEnvironment environment) : base(context)
    {
        _environment = environment;
    }

    private static decimal GenerateInitialRating()
    {
        return Random.Shared.Next(35, 51) / 10m;
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Index(string? sort = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .ThenInclude(c => c!.ParentCategory)
            .AsQueryable();

        query = sort switch
        {
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "stock_asc" => query.OrderBy(p => p.Stock),
            "stock_desc" => query.OrderByDescending(p => p.Stock),
            "rating_asc" => query.OrderBy(p => p.Rating).ThenBy(p => p.Id),
            "rating_desc" => query.OrderByDescending(p => p.Rating).ThenByDescending(p => p.Id),
            "category_asc" => query.OrderBy(p => p.Category != null ? p.Category.Name : string.Empty),
            "category_desc" => query.OrderByDescending(p => p.Category != null ? p.Category.Name : string.Empty),
            "active_asc" => query.OrderBy(p => p.IsActive),
            "active_desc" => query.OrderByDescending(p => p.IsActive),
            _ => query.OrderByDescending(p => p.Id)
        };

        ViewBag.CurrentSort = sort;

        var products = await query.ToListAsync();

        return View(products);
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(
            await _context.Categories.Where(c => c.IsActive).ToListAsync(),
            "Id",
            "Name"
        );

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .ToListAsync();

        if (!categories.Any())
        {
            TempData["Error"] = "Ürün eklemeden önce kategori oluşturmalısınız.";
            return RedirectToAction("Create", "Category");
        }

        ValidateImage(model.ImageFile);

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                categories,
                "Id",
                "Name",
                model.CategoryId
            );

            return View(model);
        }

        var imageUrl = await SaveImageAsync(model.ImageFile);

        var entity = new Product
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            Stock = model.Stock,
            ImageUrl = imageUrl,
            CategoryId = model.CategoryId,
            IsActive = model.IsActive,
            CreatedDate = DateTime.UtcNow,
            Rating = GenerateInitialRating()
        };

        _context.Products.Add(entity);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        ViewBag.Categories = new SelectList(
            await _context.Categories.Where(c => c.IsActive).ToListAsync(),
            "Id",
            "Name",
            product.CategoryId
        );

        return View(new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ExistingImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .ToListAsync();

        if (!categories.Any())
        {
            TempData["Error"] = "Önce kategori oluşturmalısınız.";
            return RedirectToAction("Create", "Category");
        }

        ValidateImage(model.ImageFile);

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                categories,
                "Id",
                "Name",
                model.CategoryId
            );

            return View(model);
        }

        var entity = await _context.Products.FindAsync(id);
        if (entity == null)
            return NotFound();

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.Price = model.Price;
        entity.Stock = model.Stock;
        entity.IsActive = model.IsActive;
        entity.CategoryId = model.CategoryId;
        entity.UpdatedDate = DateTime.UtcNow;

        if (model.ImageFile != null)
        {
            DeleteImage(entity.ImageUrl);
            entity.ImageUrl = await SaveImageAsync(model.ImageFile);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return View(product);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .ThenInclude(c => c!.ParentCategory)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
            return NotFound();

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        DeleteImage(product.ImageUrl);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> List(int? categoryId, string? search, string? sort = null, int page = 1)
    {
        int pageSize = 8;

        var query = _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c!.ParentCategory)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            var subCategoryIds = await _context.Categories
                .Where(c => c.ParentCategoryId == categoryId.Value)
                .Select(c => c.Id)
                .ToListAsync();

            subCategoryIds.Add(categoryId.Value);

            query = query.Where(p => subCategoryIds.Contains(p.CategoryId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Name.Contains(search) ||
                (p.Description != null && p.Description.Contains(search)));
        }

        query = sort switch
        {
            "newest" => query.OrderByDescending(p => p.CreatedDate),
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "rating_desc" => query.OrderByDescending(p => p.Rating).ThenByDescending(p => p.Id),
            _ => query.OrderByDescending(p => p.Id)
        };

        var totalProducts = await query.CountAsync();

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var categories = await _context.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .ToListAsync();

        var model = new ProductListViewModel
        {
            ListedProducts = products,
            Categories = categories,
            CategoryId = categoryId,
            Search = search,
            Sort = sort,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize)
        };

        return View(model);
    }

    private void ValidateImage(IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return;

        var extension = Path.GetExtension(imageFile.FileName);
        if (string.IsNullOrWhiteSpace(extension) ||
            !AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(ProductFormViewModel.ImageFile), "Lutfen JPG, PNG veya WEBP formatinda bir gorsel secin.");
        }

        if (!AllowedImageContentTypes.Contains(imageFile.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(ProductFormViewModel.ImageFile), "Yuklenen dosya gecerli bir gorsel olmali.");
        }

        if (imageFile.Length > MaxImageSizeBytes)
        {
            ModelState.AddModelError(nameof(ProductFormViewModel.ImageFile), "Gorsel boyutu 2 MB'dan buyuk olamaz.");
        }
    }

    private async Task<string?> SaveImageAsync(IFormFile? imageFile)
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

    private void DeleteImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith("/uploads/products/", StringComparison.OrdinalIgnoreCase))
            return;

        var relativePath = imageUrl.TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
