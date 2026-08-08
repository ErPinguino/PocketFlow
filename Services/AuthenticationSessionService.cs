using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PocketFlow.DTOs.Auth;

namespace PocketFlow.Services;

public interface IAuthenticationSessionService
{
    Task SignInAsync(AuthResult result, bool rememberMe);
    Task SignOutAsync();
    Task RenewOnboardingCompletedClaimAsync(ClaimsPrincipal user);
}

public class AuthenticationSessionService : IAuthenticationSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationSessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SignInAsync(AuthResult result, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()!),
            new Claim(ClaimTypes.Name, result.Name!),
            new Claim(ClaimTypes.Email, result.Email!),
            new Claim("onboarding_completed", result.OnboardingCompleted.ToString()!.ToLowerInvariant())
        };

        if (!string.IsNullOrEmpty(result.AvatarUrl))
        {
            claims.Add(new Claim("avatar_url", result.AvatarUrl));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = rememberMe };

        if (_httpContextAccessor.HttpContext != null)
        {
            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }

    public async Task SignOutAsync()
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    public async Task RenewOnboardingCompletedClaimAsync(ClaimsPrincipal user)
    {
        if (_httpContextAccessor.HttpContext == null || user.Identity is not ClaimsIdentity identity)
            return;

        var existingClaim = identity.FindFirst("onboarding_completed");
        if (existingClaim != null)
        {
            identity.RemoveClaim(existingClaim);
        }
        
        identity.AddClaim(new Claim("onboarding_completed", "true"));

        var authProperties = new AuthenticationProperties { IsPersistent = true };
        
        await _httpContextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            authProperties);
    }
}
