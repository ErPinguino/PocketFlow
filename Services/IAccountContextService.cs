using PocketFlow.Models;

namespace PocketFlow.Services;

public interface IAccountContextService
{
    Task<Account?> GetCurrentAccountAsync();
    Guid GetAuthenticatedUserId();
    string GetAuthenticatedUserName();
}
