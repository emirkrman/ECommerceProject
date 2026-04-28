using ECommerceProject.Business.Models.Accounts;

namespace ECommerceProject.Business.Services.Abstract;

public interface IAccountService
{
    Task<AccountResult> RegisterAsync(RegisterRequest request);
    Task<AccountResult> LoginAsync(LoginRequest request);
}
