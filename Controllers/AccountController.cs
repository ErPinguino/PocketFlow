using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFlow.DTOs.Auth;
using PocketFlow.Services;
using PocketFlow.ViewModels.Account;

namespace PocketFlow.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAuthenticationSessionService _authSessionService;

    public AccountController(IAuthService authService, IAuthenticationSessionService authSessionService)
    {
        _authService = authService;
        _authSessionService = authSessionService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToDashboardOrOnboarding();
        }
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToDashboardOrOnboarding();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RegisterAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Error en el registro.");
            return View(model);
        }

        await _authSessionService.SignInAsync(result, false);

        return RedirectToAction("Index", "Onboarding");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToDashboardOrOnboarding();
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToDashboardOrOnboarding();
        }

        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Error en el inicio de sesión.");
            return View(model);
        }

        await _authSessionService.SignInAsync(result, model.RememberMe);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        if (result.OnboardingCompleted == false)
        {
            return RedirectToAction("Index", "Onboarding");
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authSessionService.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToDashboardOrOnboarding()
    {
        var onboardingClaim = User.FindFirst("onboarding_completed")?.Value;
        if (onboardingClaim == "true")
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return RedirectToAction("Index", "Onboarding");
    }
}
