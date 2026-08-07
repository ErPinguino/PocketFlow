using PocketFlow.Models;
using PocketFlow.ViewModels.Dashboard;

namespace PocketFlow.ViewModels.Expenses;

public class CreateExpenseResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    
    public Guid? ExpenseId { get; set; }
    public decimal? Amount { get; set; }
    public ExpenseCategory? Category { get; set; }
    public DateTime? CreatedAt { get; set; }

    public DashboardViewModel? DashboardSummary { get; set; }
    
    public List<ExpenseWarning> Warnings { get; set; } = new();
}
