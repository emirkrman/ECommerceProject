using ECommerceProject.Data.Context;
using ECommerceProject.Data.Repositories.Abstract;
using ECommerceProject.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Data.Repositories.Concrete;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetOrderedAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.ParentCategoryId)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Category>> GetActiveAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Category>> GetParentCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Category>> GetNavigationCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.SubCategories)
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<int>> GetCategoryAndSubCategoryIdsAsync(int categoryId)
    {
        var categoryIds = await _context.Categories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == categoryId)
            .Select(c => c.Id)
            .ToListAsync();

        categoryIds.Add(categoryId);
        return categoryIds;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }

    public void Remove(Category category)
    {
        _context.Categories.Remove(category);
    }

}
