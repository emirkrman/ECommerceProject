using System.Security.Claims;
using ECommerceProject.Data.Context;
using ECommerceProject.Entity.Common;
using ECommerceProject.Entity.Concrete;
using ECommerceProject.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProject.Web.Controllers;

public class AccountController : BaseController
{
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public AccountController(AppDbContext context) : base(context)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (IsAuthenticated())
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (IsAuthenticated())
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
            return View(model);

        var normalizedEmail = NormalizeEmail(model.Email);

        var emailExists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(RegisterViewModel.Email), "Bu e-posta adresi zaten kayitli.");
            return View(model);
        }

        var user = new AppUser
        {
            FullName = model.FullName.Trim(),
            Email = normalizedEmail,
            Role = AppRoles.Customer,
            CreatedDate = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await SignInUserAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (IsAuthenticated())
            return RedirectToAction("Index", "Home");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (IsAuthenticated())
            return RedirectToAction("Index", "Home");

        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var normalizedEmail = NormalizeEmail(model.Email);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "E-posta veya sifre hatali.");
            return View(model);
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "E-posta veya sifre hatali.");
            return View(model);
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
            user.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        await SignInUserAsync(user, model.RememberMe);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(AppUser user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(14) : null
            });
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private bool IsAuthenticated()
    {
        return User.Identity?.IsAuthenticated == true;
    }
}
