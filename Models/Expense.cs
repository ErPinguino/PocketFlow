namespace PocketFlow.Models;

public enum ExpenseCategory
{
    Life,
    Whim
}

public class Expense : BaseEntity
{
    public Guid MonthlyPlanId { get; set; }
    public MonthlyPlan MonthlyPlan { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public string? Description { get; set; }
    
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
