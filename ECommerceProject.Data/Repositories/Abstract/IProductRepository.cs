using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Data.Repositories.Abstract;

public interface IProductRepository
{
    Task<List<Product>> GetPublicListAsync(
        IReadOnlyCollection<int>? categoryIds,
        string? search,
        string? sort,
        int skip,
        int take);

    Task<int> CountPublicListAsync(IReadOnlyCollection<int>? categoryIds, string? search);
    Task<Product?> GetActiveDetailsAsync(int id);
    Task<List<Product>> GetAdminListAsync(string? sort);
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByIdWithCategoryAsync(int id);
    Task AddAsync(Product product);
    void Remove(Product product);
}
