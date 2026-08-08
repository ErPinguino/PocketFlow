using System;

namespace PocketFlow.ViewModels.PiggyBanks;

public class PiggyBankListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal? TargetAmount { get; set; }
    public decimal? RemainingAmount { get; set; }
    public decimal MonthlyContribution { get; set; }
    public int? ProgressPercentage { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
}
