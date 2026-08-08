using PocketFlow.ViewModels.Dashboard;

namespace PocketFlow.ViewModels.Expenses;

public class DeleteExpenseResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public DashboardViewModel? DashboardSummary { get; set; }
}
