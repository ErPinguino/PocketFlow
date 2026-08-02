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
}

public class FinancialCalculationService : IFinancialCalculationService
{
    public decimal CalculateTotalMonthlySavings(IEnumerable<decimal> piggyBankContributions)
    {
        return piggyBankContributions.Sum();
    }

    public decimal CalculateAvailableFreePocket(decimal income, decimal fixedExpenses, decimal totalSavings)
    {
        var free = income - fixedExpenses - totalSavings;
        return free > 0 ? free : 0;
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
}
