using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PocketFlow.Data;
using PocketFlow.Models;
using PocketFlow.Repositories;

namespace PocketFlow.Services;

public class InstallmentMaterializationService : IInstallmentMaterializationService
{
    private readonly ApplicationDbContext _context;
    private readonly IMonthlyPlanRepository _monthlyPlanRepository;
    private readonly IFinancialCalculationService _calcService;
    private readonly IAppClock _clock;
    private readonly ILogger<InstallmentMaterializationService> _logger;

    public InstallmentMaterializationService(
        ApplicationDbContext context,
        IMonthlyPlanRepository monthlyPlanRepository,
        IFinancialCalculationService calcService,
        IAppClock clock,
        ILogger<InstallmentMaterializationService> logger)
    {
        _context = context;
        _monthlyPlanRepository = monthlyPlanRepository;
        _calcService = calcService;
        _clock = clock;
        _logger = logger;
    }

    public async Task MaterializePendingInstallmentsAsync(Guid accountId)
    {
        var activePlans = await _context.InstallmentPlans
            .Include(p => p.Payments)
            .Where(p => p.AccountId == accountId && p.Status == InstallmentStatus.Active)
            .ToListAsync();

        if (!activePlans.Any()) return;

        var activeMonthlyPlan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(accountId);
        if (activeMonthlyPlan == null) return;

        var localNow = _clock.LocalNow;
        bool changesMade = false;

        foreach (var plan in activePlans)
        {
            int paidCount = plan.Payments.Count;
            if (paidCount >= plan.InstallmentCount)
            {
                plan.Status = InstallmentStatus.Completed;
                plan.UpdatedAt = _clock.UtcNow;
                _context.InstallmentPlans.Update(plan);
                changesMade = true;
                continue;
            }

            var schedule = _calcService.BuildInstallmentSchedule(plan.TotalAmount, plan.InstallmentCount, plan.BaseInstallmentAmount);

            // Determine if there are pending installments to process up to localNow
            while (paidCount < plan.InstallmentCount)
            {
                int currentInstallmentNumber = paidCount + 1;
                var theoreticalDueDate = GetTheoreticalDueDate(plan.StartDate, plan.BillingDay, currentInstallmentNumber);

                if (theoreticalDueDate <= localNow)
                {
                    decimal amount = schedule[currentInstallmentNumber - 1];

                    var expense = new Expense
                    {
                        MonthlyPlanId = activeMonthlyPlan.Id,
                        Amount = amount,
                        Category = plan.Category,
                        Description = $"{plan.Description} ({currentInstallmentNumber}/{plan.InstallmentCount})",
                        Date = _clock.UtcNow
                    };
                    
                    _context.Expenses.Add(expense);

                    var payment = new InstallmentPayment
                    {
                        InstallmentPlanId = plan.Id,
                        Expense = expense,
                        InstallmentNumber = currentInstallmentNumber,
                        Amount = amount,
                        DueDate = theoreticalDueDate,
                        PaidAt = _clock.UtcNow,
                        PaymentType = InstallmentPaymentType.RegularInstallment,
                        CreatedAt = _clock.UtcNow
                    };
                    
                    _context.InstallmentPayments.Add(payment);
                    paidCount++;
                    changesMade = true;

                    if (paidCount >= plan.InstallmentCount)
                    {
                        plan.Status = InstallmentStatus.Completed;
                        plan.UpdatedAt = _clock.UtcNow;
                        _context.InstallmentPlans.Update(plan);
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        if (changesMade)
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error materializing installments for account {AccountId}", accountId);
            }
        }
    }

    private DateTime GetTheoreticalDueDate(DateTime startDate, int billingDay, int installmentNumber)
    {
        // installmentNumber 1 uses startDate's month/year (or similar logic).
        // If they already paid it early, it wouldn't be checked, but for subsequent months we add (installmentNumber - 1) months to start date.
        var targetMonthDate = startDate.AddMonths(installmentNumber - 1);
        int maxDays = DateTime.DaysInMonth(targetMonthDate.Year, targetMonthDate.Month);
        int actualDay = Math.Min(billingDay, maxDays);
        return new DateTime(targetMonthDate.Year, targetMonthDate.Month, actualDay, 0, 0, 0, DateTimeKind.Local);
    }
}
