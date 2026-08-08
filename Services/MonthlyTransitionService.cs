using Microsoft.EntityFrameworkCore;
using PocketFlow.Data;
using PocketFlow.Models;
using PocketFlow.Repositories;
using PocketFlow.ViewModels.MonthlyTransition;

namespace PocketFlow.Services;

public class MonthlyTransitionService : IMonthlyTransitionService
{
    private readonly ApplicationDbContext _context;
    private readonly IMonthlyPlanRepository _monthlyPlanRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IPiggyBankRepository _piggyBankRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IFinancialCalculationService _calcService;
    private readonly IAppClock _clock;
    private readonly ILogger<MonthlyTransitionService> _logger;

    public MonthlyTransitionService(
        ApplicationDbContext context,
        IMonthlyPlanRepository monthlyPlanRepository,
        IExpenseRepository expenseRepository,
        IPiggyBankRepository piggyBankRepository,
        IAccountRepository accountRepository,
        IFinancialCalculationService calcService,
        IAppClock clock,
        ILogger<MonthlyTransitionService> logger)
    {
        _context = context;
        _monthlyPlanRepository = monthlyPlanRepository;
        _expenseRepository = expenseRepository;
        _piggyBankRepository = piggyBankRepository;
        _accountRepository = accountRepository;
        _calcService = calcService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<decimal> CalculateLeftoverAsync(Guid accountId)
    {
        var localNow = _clock.LocalNow;
        var activePlan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(accountId);
        if (activePlan == null) return 0;

        var expenses = await _expenseRepository.GetByMonthlyPlanIdAsync(activePlan.Id);
        var totalExpenses = expenses.Sum(e => e.Amount);
        
        return activePlan.FreePocketAmount - totalExpenses;
    }

    public async Task<MonthlyTransitionSummaryViewModel> PrepareTransitionSummaryAsync(
        Guid accountId, 
        RolloverDecisionViewModel rolloverDecision, 
        MonthlyPlanReviewViewModel planReview)
    {
        var localNow = _clock.LocalNow;
        var activePlan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(accountId);
        var activePiggyBanks = await _piggyBankRepository.GetActiveByAccountIdAsync(accountId);

        int newMonth = activePlan != null ? (activePlan.Month == 12 ? 1 : activePlan.Month + 1) : localNow.Month;
        int newYear = activePlan != null ? (activePlan.Month == 12 ? activePlan.Year + 1 : activePlan.Year) : localNow.Year;

        var summary = new MonthlyTransitionSummaryViewModel
        {
            NewMonthName = GetMonthNameInSpanish(newMonth),
            NewYear = newYear,
            LeftoverAmount = rolloverDecision.LeftoverAmount,
            DestinationType = rolloverDecision.DestinationType,
            
            NewIncome = planReview.MonthlyIncome,
            NewFixedExpenses = planReview.FixedExpenses,
            NewLifeBudget = planReview.LifeBudget,
            NewWhimBudget = planReview.WhimBudget
        };

        if (rolloverDecision.DestinationType == RolloverDestinationType.PiggyBank && rolloverDecision.DestinationPiggyBankId.HasValue)
        {
            var pb = activePiggyBanks.FirstOrDefault(p => p.Id == rolloverDecision.DestinationPiggyBankId.Value);
            if (pb != null)
            {
                summary.DestinationPiggyBankName = pb.Name;
            }
        }

        var totalSavings = 0m;
        
        foreach (var pbVM in planReview.PiggyBanks)
        {
            if (pbVM.IsActive)
            {
                totalSavings += pbVM.MonthlyContribution;
                
                var pbSummary = new PiggyBankSummaryItemViewModel
                {
                    Name = pbVM.Name,
                    Icon = pbVM.Icon,
                    MonthlyContribution = pbVM.MonthlyContribution
                };

                if (rolloverDecision.DestinationType == RolloverDestinationType.PiggyBank && 
                    rolloverDecision.DestinationPiggyBankId == pbVM.Id)
                {
                    pbSummary.RolloverAddition = rolloverDecision.LeftoverAmount > 0 ? rolloverDecision.LeftoverAmount : 0;
                }
                
                summary.PiggyBanks.Add(pbSummary);
            }
        }
        
        summary.NewTotalSavings = totalSavings;
        summary.NewFreePocket = _calcService.CalculateAvailableFreePocket(planReview.MonthlyIncome, planReview.FixedExpenses, totalSavings);
        
        if (rolloverDecision.DestinationType == RolloverDestinationType.NextMonthPocket && rolloverDecision.LeftoverAmount > 0)
        {
            summary.NewFreePocket += rolloverDecision.LeftoverAmount;
        }

        return summary;
    }

    public async Task<bool> ExecuteTransitionAsync(
        Guid accountId, 
        RolloverDecisionViewModel rolloverDecision, 
        MonthlyPlanReviewViewModel planReview)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var localNow = _clock.LocalNow;
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) return false;

            // Conseguir el plan activo anterior (podría ser del mes pasado o del actual)
            var activePlan = await _context.MonthlyPlans
                .Where(p => p.AccountId == accountId && p.Status == PlanStatus.Active)
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                .FirstOrDefaultAsync();

            int newMonth;
            int newYear;

            if (activePlan != null)
            {
                newMonth = activePlan.Month == 12 ? 1 : activePlan.Month + 1;
                newYear = activePlan.Month == 12 ? activePlan.Year + 1 : activePlan.Year;
            }
            else
            {
                newMonth = localNow.Month;
                newYear = localNow.Year;
            }

            // Verificar si YA existe un plan para este mes futuro/calculado
            var existingNewPlan = await _context.MonthlyPlans.FirstOrDefaultAsync(p => p.AccountId == accountId && p.Month == newMonth && p.Year == newYear);
            if (existingNewPlan != null)
            {
                _logger.LogWarning("ExecuteTransitionAsync: Plan already exists for {Month}/{Year}", newMonth, newYear);
                return false;
            }

            if (activePlan != null)
            {
                activePlan.Status = PlanStatus.Closed;
            }

            // Crear el nuevo plan
            var totalSavings = planReview.PiggyBanks.Where(p => p.IsActive).Sum(p => p.MonthlyContribution);
            var freePocket = _calcService.CalculateAvailableFreePocket(planReview.MonthlyIncome, planReview.FixedExpenses, totalSavings);
            var weeklyBudget = _calcService.CalculateWeeklyBudget(freePocket);

            var rolloverAmount = rolloverDecision.LeftoverAmount > 0 ? rolloverDecision.LeftoverAmount : 0;

            if (rolloverDecision.DestinationType == RolloverDestinationType.NextMonthPocket)
            {
                freePocket += rolloverAmount;
            }

            var newPlan = new MonthlyPlan
            {
                AccountId = accountId,
                BasedOnPlanId = activePlan?.Id,
                Month = newMonth,
                Year = newYear,
                Income = planReview.MonthlyIncome,
                FixedExpenses = planReview.FixedExpenses,
                TotalSavings = totalSavings,
                FreePocketAmount = freePocket,
                WeeklyBudget = weeklyBudget,
                LifeBudget = planReview.LifeBudget,
                WhimBudget = planReview.WhimBudget,
                Status = PlanStatus.Active
            };

            await _context.MonthlyPlans.AddAsync(newPlan);
            await _context.SaveChangesAsync(); // Para obtener el Id

            // Procesar Rollover
            if (rolloverAmount > 0 && activePlan != null)
            {
                var rollover = new MonthlyRollover
                {
                    FromMonthlyPlanId = activePlan.Id,
                    ToMonthlyPlanId = newPlan.Id,
                    Amount = rolloverAmount,
                    DestinationType = rolloverDecision.DestinationType,
                    PiggyBankId = (rolloverDecision.DestinationType == RolloverDestinationType.PiggyBank) ? rolloverDecision.DestinationPiggyBankId : null
                };
                await _context.MonthlyRollovers.AddAsync(rollover);
            }

            // Procesar huchas
            var activePiggyBanks = await _context.PiggyBanks.Where(p => p.AccountId == accountId).ToListAsync();
            foreach (var pb in activePiggyBanks)
            {
                var pbReview = planReview.PiggyBanks.FirstOrDefault(x => x.Id == pb.Id);
                if (pbReview != null)
                {
                    if (pbReview.IsActive)
                    {
                        pb.MonthlyContribution = pbReview.MonthlyContribution;
                        pb.CurrentAmount += pbReview.MonthlyContribution;
                        
                        if (rolloverAmount > 0 && 
                            rolloverDecision.DestinationType == RolloverDestinationType.PiggyBank && 
                            rolloverDecision.DestinationPiggyBankId == pb.Id)
                        {
                            pb.CurrentAmount += rolloverAmount;
                        }
                    }
                    else
                    {
                        pb.IsActive = false;
                    }
                }
                _context.PiggyBanks.Update(pb);
            }

            // Marcar Paycheck confirmado al último día de cobro detectado
            int maxDaysInMonth = DateTime.DaysInMonth(localNow.Year, localNow.Month);
            int actualPaydayDay = account.Payday > maxDaysInMonth ? maxDaysInMonth : account.Payday;
            var paydayThisMonth = new DateTime(localNow.Year, localNow.Month, actualPaydayDay);
            
            DateTime lastPayday;
            if (localNow.Date >= paydayThisMonth)
            {
                lastPayday = paydayThisMonth;
            }
            else
            {
                var prevMonthDate = localNow.AddMonths(-1);
                int maxDaysPrevMonth = DateTime.DaysInMonth(prevMonthDate.Year, prevMonthDate.Month);
                int prevActualPayday = account.Payday > maxDaysPrevMonth ? maxDaysPrevMonth : account.Payday;
                lastPayday = new DateTime(prevMonthDate.Year, prevMonthDate.Month, prevActualPayday);
            }

            account.LastPaycheckConfirmedAt = lastPayday.ToUniversalTime();
            _context.Accounts.Update(account);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ExecuteTransitionAsync failed.");
            await transaction.RollbackAsync();
            return false;
        }
    }

    private string GetMonthNameInSpanish(int month)
    {
        return month switch
        {
            1 => "Enero", 2 => "Febrero", 3 => "Marzo", 4 => "Abril",
            5 => "Mayo", 6 => "Junio", 7 => "Julio", 8 => "Agosto",
            9 => "Septiembre", 10 => "Octubre", 11 => "Noviembre", 12 => "Diciembre",
            _ => ""
        };
    }
}
