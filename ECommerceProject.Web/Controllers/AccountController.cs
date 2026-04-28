using System.Security.Claims;
using AutoMapper;
using ECommerceProject.Business.Models.Accounts;
using ECommerceProject.Business.Services.Abstract;
using ECommerceProject.Entity.Concrete;
using ECommerceProject.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceProject.Web.Controllers;

public class AccountController : BaseController
{
    private readonly IAccountService _accountService;
    private readonly IMapper _mapper;

    public AccountController(
        IAccountService accountService,
        INavigationService navigationService,
        IMapper mapper)
        : base(navigationService)
    {
        _accountService = accountService;
        _mapper = mapper;
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

        var result = await _accountService.RegisterAsync(_mapper.Map<RegisterRequest>(model));

        if (!result.Succeeded)
        {
            ModelState.AddModelError(result.ErrorField ?? string.Empty, result.ErrorMessage!);
            return View(model);
        }

        await SignInUserAsync(result.User!, isPersistent: false);

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

        var result = await _accountService.LoginAsync(_mapper.Map<LoginRequest>(model));

        if (!result.Succeeded)
        {
            ModelState.AddModelError(result.ErrorField ?? string.Empty, result.ErrorMessage!);
            return View(model);
        }

        await SignInUserAsync(result.User!, model.RememberMe);

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

    private bool IsAuthenticated()
    {
        return User.Identity?.IsAuthenticated == true;
    }
}
