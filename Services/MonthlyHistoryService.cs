using PocketFlow.Models;
using PocketFlow.Repositories;
using PocketFlow.ViewModels.Dashboard;
using PocketFlow.ViewModels.History;

namespace PocketFlow.Services;

public class MonthlyHistoryService : IMonthlyHistoryService
{
    private readonly IAccountContextService _accountContext;
    private readonly IMonthlyPlanRepository _monthlyPlanRepository;
    private readonly IPiggyBankRepository _piggyBankRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IFinancialCalculationService _calcService;
    private readonly IAppClock _clock;
    private readonly ILogger<MonthlyHistoryService> _logger;

    public MonthlyHistoryService(
        IAccountContextService accountContext,
        IMonthlyPlanRepository monthlyPlanRepository,
        IPiggyBankRepository piggyBankRepository,
        IExpenseRepository expenseRepository,
        IFinancialCalculationService calcService,
        IAppClock clock,
        ILogger<MonthlyHistoryService> logger)
    {
        _accountContext = accountContext;
        _monthlyPlanRepository = monthlyPlanRepository;
        _piggyBankRepository = piggyBankRepository;
        _expenseRepository = expenseRepository;
        _calcService = calcService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<HistoryListViewModel?> GetHistoryListAsync()
    {
        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null) return null;

        var plans = await _monthlyPlanRepository.GetByAccountIdAsync(account.Id);
        
        var vm = new HistoryListViewModel();

        foreach (var plan in plans)
        {
            var expenses = await _expenseRepository.GetByMonthlyPlanIdAsync(plan.Id);
            var totalSpent = expenses.Sum(e => e.Amount);
            var finalBalance = plan.FreePocketAmount - totalSpent;

            vm.Plans.Add(new HistoryListItemViewModel
            {
                Id = plan.Id,
                MonthName = GetMonthNameInSpanish(plan.Month),
                Year = plan.Year,
                Status = plan.Status,
                IsActive = plan.Status == PlanStatus.Active,
                TotalSpent = totalSpent,
                FinalBalance = finalBalance
            });
        }

        return vm;
    }

    public async Task<DashboardViewModel?> GetHistoricalDashboardAsync(Guid planId)
    {
        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null) return null;

        var plan = await _monthlyPlanRepository.GetByIdAndAccountIdAsync(planId, account.Id);
        if (plan == null) return null;

        var expenses = await _expenseRepository.GetByMonthlyPlanIdAsync(plan.Id);
        
        // For history, we don't calculate "weekly spent" against the current week.
        // It's closed. We just show 0 or total. Let's just show total spent vs total budgets.
        var freePocketSpent = expenses.Sum(e => e.Amount);
        var lifeSpent = expenses.Where(e => e.Category == ExpenseCategory.Life).Sum(e => e.Amount);
        var whimSpent = expenses.Where(e => e.Category == ExpenseCategory.Whim).Sum(e => e.Amount);
        
        // Let's pretend "weekly spent" is just what they spent on average? 
        // No, the user asked to show "Weekly budget" as orientative original, and not show "remaining".
        // We will just set it to 0 for historical.
        var weeklySpent = 0m; 

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

        var finalBalance = plan.FreePocketAmount - freePocketSpent;
        string rolloverText = string.Empty;
        bool hasRollover = false;
        decimal rolloverAmount = 0m;
        string rolloverDestinationText = string.Empty;

        if (plan.Status == PlanStatus.Closed)
        {
            if (finalBalance > 0)
                rolloverText = $"Terminaste el mes con {finalBalance.ToString("C")} disponibles.";
            else if (finalBalance == 0)
                rolloverText = "Utilizaste todo tu bolsillo disponible.";
            else
                rolloverText = $"Terminaste el mes {Math.Abs(finalBalance).ToString("C")} por encima del presupuesto.";

            var rollover = await _monthlyPlanRepository.GetRolloverByFromPlanIdAsync(plan.Id);
            if (rollover != null)
            {
                hasRollover = true;
                rolloverAmount = rollover.Amount;
                rolloverDestinationText = rollover.DestinationType switch
                {
                    RolloverDestinationType.NextMonthPocket => "Añadido al bolsillo del mes siguiente",
                    RolloverDestinationType.PiggyBank => $"Traspasado a la hucha: {rollover.PiggyBank?.Name}",
                    _ => "No asignado"
                };
            }
        }

        // Para huchas históricas, solo mostramos nombre y aportación teórica, no el progreso real actual
        var piggyBanks = await _piggyBankRepository.GetActiveByAccountIdAsync(account.Id);

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
            WeeklySpent = 0,
            WeeklyRemaining = 0, // Ignored in historical

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
                CurrentAmount = 0, // Hide for historical
                TargetAmount = 0,
                MonthlyContribution = pb.MonthlyContribution, // Might not be historically accurate, but it's what we have
                ProgressPercentage = 0
            }).ToList(),
            
            ShouldAskPaydayConfirmation = false,
            
            IsHistorical = plan.Status != PlanStatus.Active,
            HistoricalFinalBalance = finalBalance,
            RolloverText = rolloverText,
            HasRollover = hasRollover,
            RolloverAmount = rolloverAmount,
            RolloverDestinationText = rolloverDestinationText
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
