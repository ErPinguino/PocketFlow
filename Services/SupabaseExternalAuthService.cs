using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PocketFlow.DTOs.Auth;
using PocketFlow.Models;
using PocketFlow.Repositories;

namespace PocketFlow.Services;

public class SupabaseExternalAuthService : ISupabaseExternalAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SupabaseExternalAuthService> _logger;

    public SupabaseExternalAuthService(IUserRepository userRepository, HttpClient httpClient, IConfiguration configuration, ILogger<SupabaseExternalAuthService> logger)
    {
        _userRepository = userRepository;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string redirectUrl, out string codeVerifier)
    {
        codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        
        var supabaseUrl = _configuration["Supabase:Url"]?.TrimEnd('/');
        
        var url = $"{supabaseUrl}/auth/v1/authorize?provider=google&redirect_to={Uri.EscapeDataString(redirectUrl)}&code_challenge={codeChallenge}&code_challenge_method=s256";
        
        return url;
    }

    public async Task<AuthResult> AuthenticateWithSupabaseAsync(string code, string codeVerifier, string redirectUrl)
    {
        var supabaseUrl = _configuration["Supabase:Url"]?.TrimEnd('/');
        var anonKey = _configuration["Supabase:AnonKey"];

        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(anonKey))
        {
            _logger.LogError("Supabase:Url o Supabase:AnonKey no estn configurados en appsettings.");
            return AuthResult.Fail("Configuración de Supabase incompleta.");
        }

        var requestBody = new
        {
            grant_type = "pkce",
            auth_code = code,
            code_verifier = codeVerifier,
            redirect_to = redirectUrl
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{supabaseUrl}/auth/v1/token?grant_type=pkce");
        request.Headers.Add("apikey", anonKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Supabase OAuth exchange failed. StatusCode: {StatusCode}, Reason: {ReasonPhrase}, Body: {Body}", 
                response.StatusCode, response.ReasonPhrase, content);
            return AuthResult.Fail("No se pudo autenticar con el proveedor externo.");
        }
        var tokenResponse = JsonSerializer.Deserialize<SupabaseTokenResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (tokenResponse?.User == null)
        {
            _logger.LogWarning("Supabase response did not contain user.");
            return AuthResult.Fail("No se pudo obtener información del usuario desde Supabase.");
        }

        if (!IsGoogleIdentity(tokenResponse.User))
        {
            _logger.LogWarning("Provider was not Google.");
            return AuthResult.Fail("La identidad devuelta no corresponde a Google.");
        }

        var supabaseUserId = tokenResponse.User.Id;
        var email = tokenResponse.User.Email;
        var name = tokenResponse.User.UserMetadata?.FullName ?? "Usuario";
        var emailVerified = IsEmailVerified(tokenResponse.User);
        var avatarUrl = tokenResponse.User.UserMetadata?.AvatarUrl ?? tokenResponse.User.UserMetadata?.Picture;

        if (string.IsNullOrEmpty(email))
        {
            return AuthResult.Fail("El proveedor no devolvió un email válido.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        // 1. Buscar por SupabaseUserId primero
        var existingSupabaseUser = await _userRepository.GetBySupabaseUserIdAsync(supabaseUserId);
        if (existingSupabaseUser != null)
        {
            if (existingSupabaseUser.AvatarUrl != avatarUrl && !string.IsNullOrEmpty(avatarUrl))
            {
                existingSupabaseUser.AvatarUrl = avatarUrl;
                _userRepository.Update(existingSupabaseUser);
                await _userRepository.SaveChangesAsync();
            }
            return AuthResult.Success(existingSupabaseUser.Id, existingSupabaseUser.Name, existingSupabaseUser.Email, existingSupabaseUser.OnboardingCompleted, existingSupabaseUser.AvatarUrl);
        }

        // 2. Buscar por Email para Account Linking
        var existingEmailUser = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (existingEmailUser != null)
        {
            if (!emailVerified)
            {
                _logger.LogWarning("Email was not confirmed.");
                return AuthResult.Fail("El email del proveedor no está verificado. Inicia sesión con contraseña.");
            }
            
            existingEmailUser.SupabaseUserId = supabaseUserId;
            if (string.IsNullOrEmpty(existingEmailUser.AvatarUrl) && !string.IsNullOrEmpty(avatarUrl))
            {
                existingEmailUser.AvatarUrl = avatarUrl;
            }
            _userRepository.Update(existingEmailUser);
            await _userRepository.SaveChangesAsync();
            
            return AuthResult.Success(existingEmailUser.Id, existingEmailUser.Name, existingEmailUser.Email, existingEmailUser.OnboardingCompleted, existingEmailUser.AvatarUrl);
        }

        // 3. Crear Nuevo Usuario (Sin PasswordHash, Onboarding falso por defecto)
        var newUser = new User
        {
            Name = name,
            Email = normalizedEmail,
            SupabaseUserId = supabaseUserId,
            PasswordHash = null,
            AvatarUrl = avatarUrl,
            OnboardingCompleted = false
        };

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return AuthResult.Success(newUser.Id, newUser.Name, newUser.Email, newUser.OnboardingCompleted, newUser.AvatarUrl);
    }

    private string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Base64UrlEncode(challengeBytes);
    }

    private string Base64UrlEncode(byte[] input)
    {
        var output = Convert.ToBase64String(input);
        output = output.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        return output;
    }

    private bool IsEmailVerified(SupabaseUser user)
    {
        if (!string.IsNullOrEmpty(user.EmailConfirmedAt))
        {
            return true; // Canonical signal
        }
        
        // Fallback or reinforcement
        return user.UserMetadata?.EmailVerified == true;
    }

    private bool IsGoogleIdentity(SupabaseUser user)
    {
        if (user.AppMetadata?.Provider == "google") return true;
        if (user.AppMetadata?.Providers != null && user.AppMetadata.Providers.Contains("google")) return true;
        
        return false;
    }
}

public class SupabaseTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("user")]
    public SupabaseUser? User { get; set; }
}

public class SupabaseUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("email_confirmed_at")]
    public string? EmailConfirmedAt { get; set; }
    
    [JsonPropertyName("app_metadata")]
    public SupabaseAppMetadata? AppMetadata { get; set; }
    
    [JsonPropertyName("user_metadata")]
    public SupabaseUserMetadata? UserMetadata { get; set; }
}

public class SupabaseAppMetadata
{
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }
    
    [JsonPropertyName("providers")]
    public List<string>? Providers { get; set; }
}

public class SupabaseUserMetadata
{
    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }
    
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }
    
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
    
    [JsonPropertyName("picture")]
    public string? Picture { get; set; }
}
