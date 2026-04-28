using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Data.Repositories.Abstract;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string normalizedEmail);
    Task<AppUser?> GetByEmailAsync(string normalizedEmail);
    Task AddAsync(AppUser user);
}
