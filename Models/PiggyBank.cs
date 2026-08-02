namespace PocketFlow.Models;

public class PiggyBank : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal MonthlyContribution { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; } = true;
}
