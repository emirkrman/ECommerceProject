using ECommerceProject.Entity.Common;

namespace ECommerceProject.Entity.Concrete;

public class AppUser : BaseEntity
{
    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = AppRoles.Customer;
}
