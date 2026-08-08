using PocketFlow.Models;

namespace PocketFlow.Repositories;

public interface IExpenseRepository
{
    Task<List<Expense>> GetByMonthlyPlanIdAsync(Guid monthlyPlanId);
    Task<List<Expense>> GetCurrentWeekByMonthlyPlanIdAsync(Guid monthlyPlanId, DateTime weekStartUtc, DateTime weekEndUtc);
    
    Task AddAsync(Expense expense);
    Task<Expense?> GetByIdAsync(Guid id);
    Task<List<Expense>> GetRecentByMonthlyPlanIdAsync(Guid monthlyPlanId, int count);
    Task<(List<Expense> Items, int TotalCount)> GetPagedByMonthlyPlanIdAsync(Guid monthlyPlanId, int page, int pageSize, ExpenseCategory? category = null, string? search = null, string? sortOrder = null);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(Expense expense);
    Task SaveChangesAsync();
}
