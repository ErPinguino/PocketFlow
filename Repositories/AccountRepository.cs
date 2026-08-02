using PocketFlow.Data;
using PocketFlow.Models;

namespace PocketFlow.Repositories;

public interface IAccountRepository
{
    Task AddAsync(Account account);
}

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
    }
}
