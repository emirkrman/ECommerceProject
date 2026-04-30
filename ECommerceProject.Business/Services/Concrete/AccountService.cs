using ECommerceProject.Business.Models.Accounts;
using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Data.Repositories.Abstract;
using ECommerceProject.Data.UnitOfWork;
using ECommerceProject.Entity.Common;
using ECommerceProject.Entity.Concrete;
using Microsoft.AspNetCore.Identity;

namespace ECommerceProject.Business.Services.Concrete;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public AccountService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountResult> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        if (await _userRepository.EmailExistsAsync(normalizedEmail))
        {
            return AccountResult.Failure("Bu e-posta adresi zaten kayıtlı.", nameof(request.Email));
        }

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Role = AppRoles.Customer,
            CreatedDate = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return AccountResult.Success(user);
    }

    public async Task<AccountResult> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (user == null)
        {
            return AccountResult.Failure("E-posta veya şifre hatalı.");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return AccountResult.Failure("E-posta veya şifre hatalı.");
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            user.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }

        return AccountResult.Success(user);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
