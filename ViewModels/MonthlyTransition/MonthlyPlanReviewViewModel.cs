using System.ComponentModel.DataAnnotations;

namespace PocketFlow.ViewModels.MonthlyTransition;

public class MonthlyPlanReviewViewModel
{
    [Required]
    [Range(0, 99999999)]
    public decimal MonthlyIncome { get; set; }

    [Required]
    [Range(0, 99999999)]
    public decimal FixedExpenses { get; set; }

    [Required]
    [Range(0, 99999999)]
    public decimal LifeBudget { get; set; }

    [Required]
    [Range(0, 99999999)]
    public decimal WhimBudget { get; set; }
    
    public List<PiggyBankReviewItemViewModel> PiggyBanks { get; set; } = new();
}

public class PiggyBankReviewItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    
    public bool IsActive { get; set; }
    
    [Range(0, 99999999)]
    public decimal MonthlyContribution { get; set; }
}
