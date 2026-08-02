using Microsoft.EntityFrameworkCore;
using PocketFlow.Data;
using PocketFlow.Models;
using System.Security.Claims;

namespace PocketFlow.Services;

public class AccountContextService : IAccountContextService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountContextService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetAuthenticatedUserId()
    {
        var idClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return idClaim != null ? Guid.Parse(idClaim) : Guid.Empty;
    }

    public string GetAuthenticatedUserName()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "Usuario";
    }

    public async Task<Account?> GetCurrentAccountAsync()
    {
        var userId = GetAuthenticatedUserId();
        if (userId == Guid.Empty) return null;

        // Devuelve la cuenta más antigua o "principal"
        return await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
