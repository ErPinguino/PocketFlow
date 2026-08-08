using Microsoft.EntityFrameworkCore;
using PocketFlow.Data;
using PocketFlow.Models;

namespace PocketFlow.Repositories;

public interface IPiggyBankRepository
{
    Task AddRangeAsync(IEnumerable<PiggyBank> piggyBanks);
    Task<List<PiggyBank>> GetActiveByAccountIdAsync(Guid accountId);
    Task<List<PiggyBank>> GetByAccountIdAsync(Guid accountId);
    Task<PiggyBank?> GetByIdAndAccountIdAsync(Guid id, Guid accountId);
    Task AddAsync(PiggyBank piggyBank);
    void Update(PiggyBank piggyBank);
    Task SaveChangesAsync();
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

    public async Task<List<PiggyBank>> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.PiggyBanks
            .AsNoTracking()
            .Where(p => p.AccountId == accountId)
            .OrderByDescending(p => p.IsActive)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PiggyBank?> GetByIdAndAccountIdAsync(Guid id, Guid accountId)
    {
        return await _context.PiggyBanks
            .FirstOrDefaultAsync(p => p.Id == id && p.AccountId == accountId);
    }

    public async Task AddAsync(PiggyBank piggyBank)
    {
        await _context.PiggyBanks.AddAsync(piggyBank);
    }

    public void Update(PiggyBank piggyBank)
    {
        _context.PiggyBanks.Update(piggyBank);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
