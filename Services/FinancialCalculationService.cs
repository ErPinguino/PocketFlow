using System;
using System.Collections.Generic;
using System.Linq;
using PocketFlow.Models;

namespace PocketFlow.Services;

public interface IFinancialCalculationService
{
    decimal CalculateTotalMonthlySavings(IEnumerable<decimal> piggyBankContributions);
    decimal CalculateAvailableFreePocket(decimal income, decimal fixedExpenses, decimal totalSavings);
    decimal CalculateWeeklyBudget(decimal freePocket);
    bool ValidatePocketBudgets(decimal availableFreePocket, decimal lifeBudget, decimal whimBudget);
    
    MonthlyStatus DetermineMonthlyStatus(decimal freePocketRemaining, decimal whimRemaining, decimal lifeRemaining, decimal weeklyRemaining);
    string GetStatusMessage(MonthlyStatus status);
    int CalculatePiggyBankProgressPercentage(decimal target, decimal current);
    
    (decimal FreePocketRemaining, decimal LifeRemaining, decimal WhimRemaining, decimal WeeklyRemaining) CalculatePlanRemainings(
        MonthlyPlan plan, 
        decimal totalExpenses, 
        decimal lifeExpenses, 
        decimal whimExpenses, 
        decimal weeklyExpenses);
        
    List<decimal> BuildInstallmentSchedule(decimal total, int count, decimal baseInstallment);
}

public class FinancialCalculationService : IFinancialCalculationService
{
    public decimal CalculateTotalMonthlySavings(IEnumerable<decimal> piggyBankContributions)
    {
        return piggyBankContributions.Sum();
    }

    public decimal CalculateAvailableFreePocket(decimal income, decimal fixedExpenses, decimal totalSavings)
    {
        return income - fixedExpenses - totalSavings;
    }

    public decimal CalculateWeeklyBudget(decimal freePocket)
    {
        return Math.Round(freePocket / 4.33m, 2, MidpointRounding.AwayFromZero);
    }

    public bool ValidatePocketBudgets(decimal availableFreePocket, decimal lifeBudget, decimal whimBudget)
    {
        return availableFreePocket == (lifeBudget + whimBudget);
    }

    public MonthlyStatus DetermineMonthlyStatus(decimal freePocketRemaining, decimal whimRemaining, decimal lifeRemaining, decimal weeklyRemaining)
    {
        if (freePocketRemaining < 0) return MonthlyStatus.OverBudget;
        if (whimRemaining < 0) return MonthlyStatus.WhimWarning;
        if (lifeRemaining < 0) return MonthlyStatus.LifeWarning;
        if (weeklyRemaining < 0) return MonthlyStatus.WeeklyWarning;
        return MonthlyStatus.Healthy;
    }

    public string GetStatusMessage(MonthlyStatus status)
    {
        return status switch
        {
            MonthlyStatus.OverBudget => "Has superado tu bolsillo libre este mes.",
            MonthlyStatus.WhimWarning => "Has superado tu presupuesto de caprichos.",
            MonthlyStatus.LifeWarning => "Has superado tu presupuesto de vida.",
            MonthlyStatus.WeeklyWarning => "Te estás pasando del presupuesto semanal.",
            _ => "Vas bien este mes."
        };
    }

    public int CalculatePiggyBankProgressPercentage(decimal target, decimal current)
    {
        if (target <= 0) return 0;
        var percentage = (int)((current / target) * 100);
        return percentage > 100 ? 100 : percentage;
    }

    public (decimal FreePocketRemaining, decimal LifeRemaining, decimal WhimRemaining, decimal WeeklyRemaining) CalculatePlanRemainings(
        MonthlyPlan plan, 
        decimal totalExpenses, 
        decimal lifeExpenses, 
        decimal whimExpenses, 
        decimal weeklyExpenses)
    {
        return (
            FreePocketRemaining: plan.FreePocketAmount - totalExpenses,
            LifeRemaining: plan.LifeBudget - lifeExpenses,
            WhimRemaining: plan.WhimBudget - whimExpenses,
            WeeklyRemaining: plan.WeeklyBudget - weeklyExpenses
        );
    }

    public List<decimal> BuildInstallmentSchedule(decimal total, int count, decimal baseInstallment)
    {
        if (total <= 0) throw new ArgumentException("Total must be > 0", nameof(total));
        if (count < 2) throw new ArgumentException("Count must be >= 2", nameof(count));
        if (baseInstallment <= 0) throw new ArgumentException("Base installment must be > 0", nameof(baseInstallment));

        var schedule = new List<decimal>();
        
        // Sum all normal installments
        decimal sumRegular = 0;
        for (int i = 0; i < count - 1; i++)
        {
            schedule.Add(baseInstallment);
            sumRegular += baseInstallment;
        }

        // Last installment absorbs the rest
        var lastInstallment = total - sumRegular;
        if (lastInstallment <= 0)
        {
            throw new InvalidOperationException("The base installment is too high to fit the total in the specified count.");
        }

        schedule.Add(lastInstallment);
        
        return schedule;
    }
}
