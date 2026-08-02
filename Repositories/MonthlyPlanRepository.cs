using Microsoft.EntityFrameworkCore;
using PocketFlow.Data;
using PocketFlow.Models;

namespace PocketFlow.Repositories;

public interface IMonthlyPlanRepository
{
    Task AddAsync(MonthlyPlan monthlyPlan);
    Task<MonthlyPlan?> GetActivePlanByAccountIdAsync(Guid accountId, int month, int year);
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

    public async Task<MonthlyPlan?> GetActivePlanByAccountIdAsync(Guid accountId, int month, int year)
    {
        return await _context.MonthlyPlans
            .AsNoTracking()
            .Where(p => p.AccountId == accountId && p.Month == month && p.Year == year && p.Status == PlanStatus.Active)
            .FirstOrDefaultAsync();
    }
}
