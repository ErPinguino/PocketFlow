using System;
using System.Collections.Generic;
using PocketFlow.Models;

namespace PocketFlow.ViewModels.Installments;

public class InstallmentPlanListItemViewModel
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; }
    public string? Provider { get; set; }
    
    public decimal TotalAmount { get; set; }
    public decimal BaseInstallmentAmount { get; set; }
    public int InstallmentCount { get; set; }
    
    public decimal PendingAmount { get; set; }
    public int PaidInstallmentsCount { get; set; }
    
    public DateTime? NextDueDate { get; set; }
    
    public InstallmentStatus Status { get; set; }
}
