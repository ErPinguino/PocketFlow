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
    private readonly ISupabaseExternalAuthService _supabaseAuthService;

    public AccountController(IAuthService authService, IAuthenticationSessionService authSessionService, ISupabaseExternalAuthService supabaseAuthService)
    {
        _authService = authService;
        _authSessionService = authSessionService;
        _supabaseAuthService = supabaseAuthService;
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
    public IActionResult GoogleLogin(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToDashboardOrOnboarding();
        }

        var redirectUrl = Url.Action("GoogleCallback", "Account", null, Request.Scheme) ?? string.Empty;
        
        var authUrl = _supabaseAuthService.GetAuthorizationUrl(redirectUrl, out var codeVerifier);
        
        // Almacenar el codeVerifier en ISession (Server-Side memory). 
        // Su presencia valida implícitamente que esta sesión inició el PKCE flow (CSRF mitigation).
        HttpContext.Session.SetString("GoogleAuth_CodeVerifier", codeVerifier);
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            HttpContext.Session.SetString("GoogleAuth_ReturnUrl", returnUrl);
        }

        return Redirect(authUrl);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GoogleCallback(string? code, string? error, string? error_description)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToDashboardOrOnboarding();
        }

        if (!string.IsNullOrEmpty(error))
        {
            ModelState.AddModelError(string.Empty, error_description ?? "Error devuelto por el proveedor de identidad.");
            return View("Login");
        }

        if (string.IsNullOrEmpty(code))
        {
            ModelState.AddModelError(string.Empty, "Respuesta inválida desde el proveedor de identidad (falta código).");
            return View("Login");
        }

        // Validación estricta de PKCE Session
        var codeVerifier = HttpContext.Session.GetString("GoogleAuth_CodeVerifier");
        var returnUrl = HttpContext.Session.GetString("GoogleAuth_ReturnUrl");

        // Purgar la sesión inmediatamente (Single-use)
        HttpContext.Session.Remove("GoogleAuth_CodeVerifier");
        HttpContext.Session.Remove("GoogleAuth_ReturnUrl");

        if (string.IsNullOrEmpty(codeVerifier))
        {
            ModelState.AddModelError(string.Empty, "La sesión de validación PKCE ha expirado o es inválida. Por favor, intenta de nuevo.");
            return View("Login");
        }

        var redirectUrl = Url.Action("GoogleCallback", "Account", null, Request.Scheme) ?? string.Empty;
        var result = await _supabaseAuthService.AuthenticateWithSupabaseAsync(code, codeVerifier, redirectUrl);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "No se pudo iniciar sesión con Google.");
            return View("Login");
        }

        // Crear la sesión de PocketFlow
        await _authSessionService.SignInAsync(result, false);

        if (result.OnboardingCompleted == false)
        {
            return RedirectToAction("Index", "Onboarding");
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
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
