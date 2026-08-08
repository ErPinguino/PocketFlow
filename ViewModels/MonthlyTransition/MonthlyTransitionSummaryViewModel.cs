using PocketFlow.Models;

namespace PocketFlow.ViewModels.MonthlyTransition;

public class MonthlyTransitionSummaryViewModel
{
    public string NewMonthName { get; set; } = string.Empty;
    public int NewYear { get; set; }

    public decimal LeftoverAmount { get; set; }
    public RolloverDestinationType DestinationType { get; set; }
    public string DestinationPiggyBankName { get; set; } = string.Empty;

    public decimal NewIncome { get; set; }
    public decimal NewFixedExpenses { get; set; }
    public decimal NewTotalSavings { get; set; }
    public decimal NewFreePocket { get; set; }
    public decimal NewLifeBudget { get; set; }
    public decimal NewWhimBudget { get; set; }
    
    public List<PiggyBankSummaryItemViewModel> PiggyBanks { get; set; } = new();
}

public class PiggyBankSummaryItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public decimal MonthlyContribution { get; set; }
    public decimal RolloverAddition { get; set; }
}
