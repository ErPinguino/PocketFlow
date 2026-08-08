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

    public async Task<Expense?> GetByIdAsync(Guid id)
    {
        return await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
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

    public async Task<(List<Expense> Items, int TotalCount)> GetPagedByMonthlyPlanIdAsync(Guid monthlyPlanId, int page, int pageSize, ExpenseCategory? category = null, string? search = null, string? sortOrder = null)
    {
        var query = _context.Expenses
            .AsNoTracking()
            .Where(e => e.MonthlyPlanId == monthlyPlanId);

        if (category.HasValue)
        {
            query = query.Where(e => e.Category == category.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Case-insensitive search supported by EF Core / PostgreSQL ILIKE
            query = query.Where(e => e.Description != null && e.Description.ToLower().Contains(search.ToLower()));
        }

        var totalCount = await query.CountAsync();

        switch (sortOrder)
        {
            case "oldest":
                query = query.OrderBy(e => e.CreatedAt);
                break;
            case "highest":
                query = query.OrderByDescending(e => e.Amount).ThenByDescending(e => e.CreatedAt);
                break;
            case "lowest":
                query = query.OrderBy(e => e.Amount).ThenByDescending(e => e.CreatedAt);
                break;
            case "newest":
            default:
                query = query.OrderByDescending(e => e.CreatedAt);
                break;
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task UpdateAsync(Expense expense)
    {
        _context.Expenses.Update(expense);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Expense expense)
    {
        _context.Expenses.Remove(expense);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
