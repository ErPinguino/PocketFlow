using PocketFlow.Models;

namespace PocketFlow.Repositories;

public interface IExpenseRepository
{
    Task<List<Expense>> GetByMonthlyPlanIdAsync(Guid monthlyPlanId);
    Task<List<Expense>> GetCurrentWeekByMonthlyPlanIdAsync(Guid monthlyPlanId, DateTime weekStartUtc, DateTime weekEndUtc);
    
    Task AddAsync(Expense expense);
    Task<List<Expense>> GetRecentByMonthlyPlanIdAsync(Guid monthlyPlanId, int count);
    Task<(List<Expense> Items, int TotalCount)> GetPagedByMonthlyPlanIdAsync(Guid monthlyPlanId, int page, int pageSize, ExpenseCategory? category = null);
    Task SaveChangesAsync();
}
