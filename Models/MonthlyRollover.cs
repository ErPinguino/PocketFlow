namespace PocketFlow.Models;

public enum RolloverDestinationType
{
    PiggyBank,
    NextMonthPocket,
    Unassigned
}

public class MonthlyRollover : BaseEntity
{
    public Guid FromMonthlyPlanId { get; set; }
    public MonthlyPlan FromMonthlyPlan { get; set; } = null!;
    
    public Guid? ToMonthlyPlanId { get; set; }
    public MonthlyPlan? ToMonthlyPlan { get; set; }
    
    public Guid? PiggyBankId { get; set; }
    public PiggyBank? PiggyBank { get; set; }
    
    public decimal Amount { get; set; }
    public RolloverDestinationType DestinationType { get; set; }
}
