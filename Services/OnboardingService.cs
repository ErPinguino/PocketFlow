using Microsoft.EntityFrameworkCore;
using PocketFlow.Data;
using PocketFlow.Models;
using PocketFlow.Repositories;
using PocketFlow.ViewModels.Onboarding;

namespace PocketFlow.Services;

public interface IOnboardingService
{
    Task<bool> CompleteOnboardingAsync(Guid userId, OnboardingSummaryViewModel finalState);
}

public class OnboardingService : IOnboardingService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IPiggyBankRepository _piggyBankRepository;
    private readonly IMonthlyPlanRepository _monthlyPlanRepository;
    private readonly IFinancialCalculationService _calcService;
    private readonly IAppClock _clock;
    private readonly ILogger<OnboardingService> _logger;

    public OnboardingService(
        ApplicationDbContext context,
        IUserRepository userRepository,
        IAccountRepository accountRepository,
        IPiggyBankRepository piggyBankRepository,
        IMonthlyPlanRepository monthlyPlanRepository,
        IFinancialCalculationService calcService,
        IAppClock clock,
        ILogger<OnboardingService> logger)
    {
        _context = context;
        _userRepository = userRepository;
        _accountRepository = accountRepository;
        _piggyBankRepository = piggyBankRepository;
        _monthlyPlanRepository = monthlyPlanRepository;
        _calcService = calcService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<bool> CompleteOnboardingAsync(Guid userId, OnboardingSummaryViewModel finalState)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.OnboardingCompleted)
            {
                _logger.LogWarning("CompleteOnboarding: User {UserId} no encontrado o ya completó onboarding.", userId);
                return false;
            }

            var totalSavings = _calcService.CalculateTotalMonthlySavings(finalState.PiggyBanks.Select(p => p.MonthlyContribution));
            var availableFree = _calcService.CalculateAvailableFreePocket(finalState.MonthlyIncome, finalState.FixedExpenses, totalSavings);
            var isPocketValid = _calcService.ValidatePocketBudgets(availableFree, finalState.LifeBudget, finalState.WhimBudget);

            if (!isPocketValid)
            {
                _logger.LogWarning("CompleteOnboarding: Presupuestos inválidos para el usuario {UserId}.", userId);
                return false;
            }

            var localNow = _clock.LocalNow;
            
            // Set to the most recent payday before localNow so it doesn't trigger immediately
            int maxDaysInMonth = DateTime.DaysInMonth(localNow.Year, localNow.Month);
            int actualPaydayDay = finalState.Payday > maxDaysInMonth ? maxDaysInMonth : finalState.Payday;
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
                int prevActualPayday = finalState.Payday > maxDaysPrevMonth ? maxDaysPrevMonth : finalState.Payday;
                lastPayday = new DateTime(prevMonthDate.Year, prevMonthDate.Month, prevActualPayday);
            }

            var account = new Account
            {
                UserId = userId,
                Name = finalState.AccountName,
                Currency = finalState.Currency,
                MonthlyIncome = finalState.MonthlyIncome,
                Payday = finalState.Payday,
                LastPaycheckConfirmedAt = lastPayday.ToUniversalTime()
            };
            await _accountRepository.AddAsync(account);
            await _context.SaveChangesAsync();

            var piggyBanks = new List<PiggyBank>();
            foreach (var pb in finalState.PiggyBanks)
            {
                piggyBanks.Add(new PiggyBank
                {
                    AccountId = account.Id,
                    Name = pb.Name,
                    CurrentAmount = pb.CurrentAmount,
                    TargetAmount = pb.TargetAmount,
                    MonthlyContribution = pb.MonthlyContribution,
                    Icon = pb.Icon
                });
            }
            if (piggyBanks.Any())
            {
                await _piggyBankRepository.AddRangeAsync(piggyBanks);
            }

            var weeklyBudget = _calcService.CalculateWeeklyBudget(availableFree);

            var monthlyPlan = new MonthlyPlan
            {
                AccountId = account.Id,
                BasedOnPlanId = null,
                Month = localNow.Month,
                Year = localNow.Year,
                Income = finalState.MonthlyIncome,
                FixedExpenses = finalState.FixedExpenses,
                TotalSavings = totalSavings,
                FreePocketAmount = availableFree,
                LifeBudget = finalState.LifeBudget,
                WhimBudget = finalState.WhimBudget,
                WeeklyBudget = weeklyBudget,
                Status = PlanStatus.Active
            };
            await _monthlyPlanRepository.AddAsync(monthlyPlan);

            user.OnboardingCompleted = true;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error crítico durante el onboarding del usuario {UserId}", userId);
            return false;
        }
    }
}
