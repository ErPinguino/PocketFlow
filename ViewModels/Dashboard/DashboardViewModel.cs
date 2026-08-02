using PocketFlow.Models;

namespace PocketFlow.ViewModels.Dashboard;

public class DashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }

    public decimal FreePocketInitial { get; set; }
    public decimal FreePocketSpent { get; set; }
    public decimal FreePocketRemaining { get; set; }

    public decimal WeeklyBudget { get; set; }
    public decimal WeeklySpent { get; set; }
    public decimal WeeklyRemaining { get; set; }

    public decimal LifeBudget { get; set; }
    public decimal LifeSpent { get; set; }
    public decimal LifeRemaining { get; set; }

    public decimal WhimBudget { get; set; }
    public decimal WhimSpent { get; set; }
    public decimal WhimRemaining { get; set; }

    public MonthlyStatus MonthlyStatus { get; set; }
    public string StatusMessage { get; set; } = string.Empty;

    public List<PiggyBankDashboardItemViewModel> PiggyBanks { get; set; } = new();
}
