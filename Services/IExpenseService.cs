using PocketFlow.ViewModels.Expenses;

namespace PocketFlow.Services;

public interface IExpenseService
{
    Task<CreateExpenseResult> CreateExpenseAsync(CreateExpenseViewModel model);
    Task<CreateExpenseResult> UpdateExpenseAsync(UpdateExpenseViewModel model);
    Task<DeleteExpenseResult> DeleteExpenseAsync(Guid expenseId);
}
