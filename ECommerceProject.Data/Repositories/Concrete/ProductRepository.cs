using ECommerceProject.Data.Context;
using ECommerceProject.Data.Repositories.Abstract;
using ECommerceProject.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Data.Repositories.Concrete;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetPublicListAsync(
        IReadOnlyCollection<int>? categoryIds,
        string? search,
        string? sort,
        int skip,
        int take)
    {
        var query = BuildPublicQuery(categoryIds, search);

        return await ApplyPublicSort(query, sort)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountPublicListAsync(IReadOnlyCollection<int>? categoryIds, string? search)
    {
        var query = BuildPublicQuery(categoryIds, search);
        return await query.CountAsync();
    }

    public async Task<Product?> GetActiveDetailsAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .ThenInclude(c => c!.ParentCategory)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
    }

    public async Task<List<Product>> GetAdminListAsync(string? sort)
    {
        var query = _context.Products
            .AsNoTracking()
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

        return await query.ToListAsync();
    }

    public async Task<List<Product>> GetLatestActiveAsync(int count)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public void Remove(Product product)
    {
        _context.Products.Remove(product);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    private IQueryable<Product> BuildPublicQuery(IReadOnlyCollection<int>? categoryIds, string? search)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .ThenInclude(c => c!.ParentCategory)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (categoryIds is { Count: > 0 })
        {
            query = query.Where(p => categoryIds.Contains(p.CategoryId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Name.Contains(search) ||
                (p.Description != null && p.Description.Contains(search)));
        }

        return query;
    }

    private static IQueryable<Product> ApplyPublicSort(IQueryable<Product> query, string? sort)
    {
        return sort switch
        {
            "newest" => query.OrderByDescending(p => p.CreatedDate),
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "rating_desc" => query.OrderByDescending(p => p.Rating).ThenByDescending(p => p.Id),
            _ => query.OrderByDescending(p => p.Id)
        };
    }
}
