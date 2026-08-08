using Microsoft.EntityFrameworkCore;
using PocketFlow.Data;
using PocketFlow.Models;

namespace PocketFlow.Repositories;

public interface IMonthlyPlanRepository
{
    Task AddAsync(MonthlyPlan monthlyPlan);
    Task<MonthlyPlan?> GetActivePlanByAccountIdAsync(Guid accountId);
    Task<List<MonthlyPlan>> GetByAccountIdAsync(Guid accountId);
    Task<MonthlyPlan?> GetByIdAndAccountIdAsync(Guid id, Guid accountId);
    Task<MonthlyRollover?> GetRolloverByFromPlanIdAsync(Guid fromPlanId);
}

public class MonthlyPlanRepository : IMonthlyPlanRepository
{
    private readonly ApplicationDbContext _context;

    public MonthlyPlanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(MonthlyPlan monthlyPlan)
    {
        await _context.MonthlyPlans.AddAsync(monthlyPlan);
    }

    public async Task<MonthlyPlan?> GetActivePlanByAccountIdAsync(Guid accountId)
    {
        return await _context.MonthlyPlans
            .AsNoTracking()
            .Where(p => p.AccountId == accountId && p.Status == PlanStatus.Active)
            .FirstOrDefaultAsync();
    }

    public async Task<List<MonthlyPlan>> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.MonthlyPlans
            .AsNoTracking()
            .Where(p => p.AccountId == accountId)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync();
    }

    public async Task<MonthlyPlan?> GetByIdAndAccountIdAsync(Guid id, Guid accountId)
    {
        return await _context.MonthlyPlans
            .AsNoTracking()
            .Where(p => p.Id == id && p.AccountId == accountId)
            .FirstOrDefaultAsync();
    }

    public async Task<MonthlyRollover?> GetRolloverByFromPlanIdAsync(Guid fromPlanId)
    {
        return await _context.MonthlyRollovers
            .Include(r => r.PiggyBank)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.FromMonthlyPlanId == fromPlanId);
    }
}
