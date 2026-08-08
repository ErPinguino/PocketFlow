using PocketFlow.Models;
using PocketFlow.Repositories;
using PocketFlow.ViewModels.Dashboard;

namespace PocketFlow.Services;

public class DashboardService : IDashboardService
{
    private readonly IAccountContextService _accountContext;
    private readonly IMonthlyPlanRepository _monthlyPlanRepository;
    private readonly IPiggyBankRepository _piggyBankRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IFinancialCalculationService _calcService;
    private readonly IAppClock _clock;
    private readonly IPaydayService _paydayService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IAccountContextService accountContext,
        IMonthlyPlanRepository monthlyPlanRepository,
        IPiggyBankRepository piggyBankRepository,
        IExpenseRepository expenseRepository,
        IFinancialCalculationService calcService,
        IAppClock clock,
        IPaydayService paydayService,
        ILogger<DashboardService> logger)
    {
        _accountContext = accountContext;
        _monthlyPlanRepository = monthlyPlanRepository;
        _piggyBankRepository = piggyBankRepository;
        _expenseRepository = expenseRepository;
        _calcService = calcService;
        _clock = clock;
        _paydayService = paydayService;
        _logger = logger;
    }

    public async Task<DashboardViewModel?> GetDashboardAsync()
    {
        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null)
        {
            _logger.LogWarning("GetDashboardAsync: No account found for user.");
            return null;
        }

        var localNow = _clock.LocalNow;
        var plan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(account.Id);
        if (plan == null)
        {
            _logger.LogWarning("GetDashboardAsync: No active plan found for account {AccountId}.", account.Id);
            return null;
        }

        var expenses = await _expenseRepository.GetByMonthlyPlanIdAsync(plan.Id);
        
        var weekLimits = _clock.GetCurrentWeekLimitsUtc();
        var weeklyExpenses = await _expenseRepository.GetCurrentWeekByMonthlyPlanIdAsync(plan.Id, weekLimits.StartUtc, weekLimits.EndUtc);

        var piggyBanks = await _piggyBankRepository.GetActiveByAccountIdAsync(account.Id);

        var freePocketSpent = expenses.Sum(e => e.Amount);
        var lifeSpent = expenses.Where(e => e.Category == ExpenseCategory.Life).Sum(e => e.Amount);
        var whimSpent = expenses.Where(e => e.Category == ExpenseCategory.Whim).Sum(e => e.Amount);
        var weeklySpent = weeklyExpenses.Sum(e => e.Amount);

        var remainings = _calcService.CalculatePlanRemainings(
            plan,
            freePocketSpent,
            lifeSpent,
            whimSpent,
            weeklySpent
        );

        var status = _calcService.DetermineMonthlyStatus(
            remainings.FreePocketRemaining,
            remainings.WhimRemaining,
            remainings.LifeRemaining,
            remainings.WeeklyRemaining
        );

        var model = new DashboardViewModel
        {
            UserName = _accountContext.GetAuthenticatedUserName(),
            AccountId = account.Id,
            AccountName = account.Name,
            Currency = account.Currency,
            Month = GetMonthNameInSpanish(plan.Month),
            Year = plan.Year,
            
            FreePocketInitial = plan.FreePocketAmount,
            FreePocketSpent = freePocketSpent,
            FreePocketRemaining = remainings.FreePocketRemaining,

            WeeklyBudget = plan.WeeklyBudget,
            WeeklySpent = weeklySpent,
            WeeklyRemaining = remainings.WeeklyRemaining,

            LifeBudget = plan.LifeBudget,
            LifeSpent = lifeSpent,
            LifeRemaining = remainings.LifeRemaining,

            WhimBudget = plan.WhimBudget,
            WhimSpent = whimSpent,
            WhimRemaining = remainings.WhimRemaining,

            MonthlyStatus = status,
            StatusMessage = _calcService.GetStatusMessage(status),
            
            PiggyBanks = piggyBanks.Select(pb => new PiggyBankDashboardItemViewModel
            {
                Id = pb.Id,
                Name = pb.Name,
                Icon = pb.Icon,
                CurrentAmount = pb.CurrentAmount,
                TargetAmount = pb.TargetAmount,
                MonthlyContribution = pb.MonthlyContribution,
                ProgressPercentage = pb.TargetAmount.HasValue ? _calcService.CalculatePiggyBankProgressPercentage(pb.TargetAmount.Value, pb.CurrentAmount) : null
            }).ToList(),
            
            ShouldAskPaydayConfirmation = _paydayService.ShouldAskPaydayConfirmation(account)
        };

        return model;
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
