namespace PocketFlow.ViewModels.Onboarding;

public class OnboardingSummaryViewModel
{
    public string AccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public int Payday { get; set; }
    
    public decimal FixedExpenses { get; set; }
    public decimal TotalMonthlySavings { get; set; }
    
    public List<OnboardingPiggyBankItemViewModel> PiggyBanks { get; set; } = new();
    
    public decimal AvailableFreePocket { get; set; }
    public decimal LifeBudget { get; set; }
    public decimal WhimBudget { get; set; }
    public decimal WeeklyBudget { get; set; }
}
