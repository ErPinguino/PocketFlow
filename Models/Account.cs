namespace PocketFlow.Models;

public class Account : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "EUR";
    public int Payday { get; set; } = 1;
    public decimal MonthlyIncome { get; set; }
    
    public ICollection<PiggyBank> PiggyBanks { get; set; } = new List<PiggyBank>();
    public ICollection<MonthlyPlan> MonthlyPlans { get; set; } = new List<MonthlyPlan>();
    public ICollection<InstallmentPlan> InstallmentPlans { get; set; } = new List<InstallmentPlan>();
    
    // Notification Preferences
    public bool NotifyPayday { get; set; } = true;
    public bool NotifyWeeklyBudget { get; set; } = true;
    public bool NotifyPiggyBanks { get; set; } = true;
    public bool NotifyExpenseReminders { get; set; } = false;
    
    public DateTime? LastPaycheckConfirmedAt { get; set; }
}
