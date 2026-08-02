using Microsoft.EntityFrameworkCore;
using PocketFlow.Data;
using PocketFlow.Models;

namespace PocketFlow.Repositories;

public interface IPiggyBankRepository
{
    Task AddRangeAsync(IEnumerable<PiggyBank> piggyBanks);
    Task<List<PiggyBank>> GetActiveByAccountIdAsync(Guid accountId);
}

public class PiggyBankRepository : IPiggyBankRepository
{
    private readonly ApplicationDbContext _context;

    public PiggyBankRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<PiggyBank> piggyBanks)
    {
        await _context.PiggyBanks.AddRangeAsync(piggyBanks);
    }

    public async Task<List<PiggyBank>> GetActiveByAccountIdAsync(Guid accountId)
    {
        return await _context.PiggyBanks
            .AsNoTracking()
            .Where(p => p.AccountId == accountId && p.IsActive)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
    }
}
