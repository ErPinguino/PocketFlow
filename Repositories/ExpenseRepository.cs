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

    public async Task AddAsync(Expense expense)
    {
        await _context.Expenses.AddAsync(expense);
    }

    public async Task<List<Expense>> GetRecentByMonthlyPlanIdAsync(Guid monthlyPlanId, int count)
    {
        return await _context.Expenses
            .AsNoTracking()
            .Where(e => e.MonthlyPlanId == monthlyPlanId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(List<Expense> Items, int TotalCount)> GetPagedByMonthlyPlanIdAsync(Guid monthlyPlanId, int page, int pageSize, ExpenseCategory? category = null)
    {
        var query = _context.Expenses
            .AsNoTracking()
            .Where(e => e.MonthlyPlanId == monthlyPlanId);

        if (category.HasValue)
        {
            query = query.Where(e => e.Category == category.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
