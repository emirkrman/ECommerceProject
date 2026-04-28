using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Business.Services.Abstract;

public interface INavigationService
{
    Task<List<Category>> GetNavigationCategoriesAsync();
}
