using System;
using System.Collections.Generic;

namespace PocketFlow.Models;

public class InstallmentPlan : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    public string Description { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; }
    public string? Provider { get; set; }
    
    public decimal TotalAmount { get; set; }
    public int InstallmentCount { get; set; }
    public decimal BaseInstallmentAmount { get; set; }
    public int BillingDay { get; set; }
    public DateTime StartDate { get; set; }
    
    public InstallmentStatus Status { get; set; } = InstallmentStatus.Active;
    
    public ICollection<InstallmentPayment> Payments { get; set; } = new List<InstallmentPayment>();
}
