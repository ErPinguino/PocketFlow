using PocketFlow.DTOs.Auth;

namespace PocketFlow.Services;

public interface ISupabaseExternalAuthService
{
    string GetAuthorizationUrl(string redirectUrl, out string codeVerifier);
    Task<AuthResult> AuthenticateWithSupabaseAsync(string code, string codeVerifier, string redirectUrl);
}
