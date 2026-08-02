using Microsoft.EntityFrameworkCore;
using PocketFlow.Data;
using PocketFlow.Models;

namespace PocketFlow.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ApplicationDbContext _context;

    public ExpenseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Expense>> GetByMonthlyPlanIdAsync(Guid monthlyPlanId)
    {
        return await _context.Expenses
            .AsNoTracking()
            .Where(e => e.MonthlyPlanId == monthlyPlanId)
            .ToListAsync();
    }

    public async Task<List<Expense>> GetCurrentWeekByMonthlyPlanIdAsync(Guid monthlyPlanId, DateTime weekStartUtc, DateTime weekEndUtc)
    {
        return await _context.Expenses
            .AsNoTracking()
            .Where(e => e.MonthlyPlanId == monthlyPlanId && e.CreatedAt >= weekStartUtc && e.CreatedAt <= weekEndUtc)
            .ToListAsync();
    }
}
