using PocketFlow.Models;

namespace PocketFlow.Repositories;

public interface IExpenseRepository
{
    Task<List<Expense>> GetByMonthlyPlanIdAsync(Guid monthlyPlanId);
    Task<List<Expense>> GetCurrentWeekByMonthlyPlanIdAsync(Guid monthlyPlanId, DateTime weekStartUtc, DateTime weekEndUtc);
}
