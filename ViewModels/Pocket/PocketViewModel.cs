using PocketFlow.Models;

namespace PocketFlow.ViewModels.Pocket;

public class PocketViewModel
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = "EUR";
    
    public decimal FreePocketRemaining { get; set; }
    public decimal WeeklyRemaining { get; set; }
    public decimal LifeRemaining { get; set; }
    public decimal WhimRemaining { get; set; }
    
    public ExpenseCategory? ActiveFilter { get; set; }
    
    // Future-proofing para planes históricos
    public bool IsActivePlan { get; set; } = true;
    
    public List<ExpenseListItemViewModel> Expenses { get; set; } = new();
    
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
}
