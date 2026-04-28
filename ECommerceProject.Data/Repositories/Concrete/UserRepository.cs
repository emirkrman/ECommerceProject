using ECommerceProject.Data.Context;
using ECommerceProject.Data.Repositories.Abstract;
using ECommerceProject.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Data.Repositories.Concrete;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EmailExistsAsync(string normalizedEmail)
    {
        return await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
    }

    public async Task<AppUser?> GetByEmailAsync(string normalizedEmail)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
    }

    public async Task AddAsync(AppUser user)
    {
        await _context.Users.AddAsync(user);
    }

}
