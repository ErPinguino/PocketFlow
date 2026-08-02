using PocketFlow.DTOs.Auth;
using PocketFlow.ViewModels.Account;

namespace PocketFlow.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterViewModel model);
    Task<AuthResult> LoginAsync(LoginViewModel model);
}
