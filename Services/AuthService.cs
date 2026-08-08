using PocketFlow.DTOs.Auth;
using PocketFlow.Models;
using PocketFlow.Repositories;
using PocketFlow.ViewModels.Account;

namespace PocketFlow.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword("dummy");

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResult> RegisterAsync(RegisterViewModel model)
    {
        var normalizedEmail = model.Email.Trim().ToLowerInvariant();

        if (await _userRepository.EmailExistsAsync(normalizedEmail))
        {
            return AuthResult.Fail("Ya existe una cuenta con ese correo electrónico.");
        }

        var user = new User
        {
            Name = model.Name,
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            OnboardingCompleted = false
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return AuthResult.Success(user.Id, user.Name, user.Email, user.OnboardingCompleted);
    }

    public async Task<AuthResult> LoginAsync(LoginViewModel model)
    {
        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        
        bool isPasswordValid = false;
        
        if (user != null && !string.IsNullOrEmpty(user.PasswordHash))
        {
            isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
        }
        else
        {
            // Dummy check to prevent timing attacks
            BCrypt.Net.BCrypt.Verify(model.Password, DummyHash);
        }

        if (user == null || !isPasswordValid)
        {
            return AuthResult.Fail("Correo electrónico o contraseña incorrectos.");
        }

        return AuthResult.Success(user.Id, user.Name, user.Email, user.OnboardingCompleted);
    }
}
