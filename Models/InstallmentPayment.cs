using System;

namespace PocketFlow.Models;

public class InstallmentPayment : BaseEntity
{
    public Guid InstallmentPlanId { get; set; }
    public InstallmentPlan InstallmentPlan { get; set; } = null!;
    
    public Guid? ExpenseId { get; set; }
    public Expense? Expense { get; set; }
    
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime PaidAt { get; set; }
    
    public InstallmentPaymentType PaymentType { get; set; } = InstallmentPaymentType.RegularInstallment;
}
