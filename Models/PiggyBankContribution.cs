using System;

namespace PocketFlow.Models;

public class PiggyBankContribution : BaseEntity
{
    public Guid PiggyBankId { get; set; }
    public PiggyBank PiggyBank { get; set; } = null!;

    public Guid MonthlyPlanId { get; set; }
    public MonthlyPlan MonthlyPlan { get; set; } = null!;

    public decimal Amount { get; set; }
    public ContributionType Type { get; set; }
}
