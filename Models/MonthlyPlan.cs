namespace PocketFlow.Models;

public enum PlanStatus
{
    Active,
    Closed
}

public class MonthlyPlan : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    public Guid? BasedOnPlanId { get; set; }
    public MonthlyPlan? BasedOnPlan { get; set; }
    
    public int Month { get; set; }
    public int Year { get; set; }
    
    public PlanStatus Status { get; set; } = PlanStatus.Active;
    
    public decimal Income { get; set; }
    public decimal FixedExpenses { get; set; }
    public decimal TotalSavings { get; set; }
    public decimal FreePocketAmount { get; set; }
    public decimal WeeklyBudget { get; set; }
    public decimal LifeBudget { get; set; }
    public decimal WhimBudget { get; set; }
    
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
