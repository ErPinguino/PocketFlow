using PocketFlow.Models;
using PocketFlow.Repositories;
using PocketFlow.ViewModels.Expenses;

namespace PocketFlow.Services;

public class ExpenseService : IExpenseService
{
    private readonly IAccountContextService _accountContext;
    private readonly IMonthlyPlanRepository _monthlyPlanRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IFinancialCalculationService _calcService;
    private readonly IAppClock _clock;
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(
        IAccountContextService accountContext,
        IMonthlyPlanRepository monthlyPlanRepository,
        IExpenseRepository expenseRepository,
        IFinancialCalculationService calcService,
        IAppClock clock,
        IDashboardService dashboardService,
        ILogger<ExpenseService> logger)
    {
        _accountContext = accountContext;
        _monthlyPlanRepository = monthlyPlanRepository;
        _expenseRepository = expenseRepository;
        _calcService = calcService;
        _clock = clock;
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public async Task<CreateExpenseResult> CreateExpenseAsync(CreateExpenseViewModel model)
    {
        var result = new CreateExpenseResult();

        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null)
        {
            result.ErrorMessage = "No se encontró la cuenta del usuario.";
            return result;
        }

        var localNow = _clock.LocalNow;
        var plan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(account.Id);
        
        if (plan == null)
        {
            result.ErrorMessage = "No hay un plan mensual activo para registrar el gasto.";
            return result;
        }
        
        if (plan.Status != PlanStatus.Active)
        {
            result.ErrorMessage = "El plan mensual no está activo.";
            return result;
        }

        var nowUtc = _clock.UtcNow;
        var expense = new Expense
        {
            MonthlyPlanId = plan.Id,
            Amount = model.Amount,
            Category = model.Category,
            Description = model.Description,
            CreatedAt = nowUtc,
            Date = nowUtc
        };

        await _expenseRepository.AddAsync(expense);
        await _expenseRepository.SaveChangesAsync();

        result.Succeeded = true;
        result.ExpenseId = expense.Id;
        result.Amount = expense.Amount;
        result.Category = expense.Category;
        result.CreatedAt = expense.CreatedAt;

        // Recalcular advertencias
        var expenses = await _expenseRepository.GetByMonthlyPlanIdAsync(plan.Id);
        
        var weekLimits = _clock.GetCurrentWeekLimitsUtc();
        var weeklyExpenses = await _expenseRepository.GetCurrentWeekByMonthlyPlanIdAsync(plan.Id, weekLimits.StartUtc, weekLimits.EndUtc);

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

        // El orden solicitado en los requerimientos:
        if (remainings.FreePocketRemaining < 0) result.Warnings.Add(ExpenseWarning.FreePocketExceeded);
        if (expense.Category == ExpenseCategory.Whim && remainings.WhimRemaining < 0) result.Warnings.Add(ExpenseWarning.WhimBudgetExceeded);
        if (expense.Category == ExpenseCategory.Life && remainings.LifeRemaining < 0) result.Warnings.Add(ExpenseWarning.LifeBudgetExceeded);
        if (remainings.WeeklyRemaining < 0) result.Warnings.Add(ExpenseWarning.WeeklyBudgetExceeded);

        // Actualizar dashboard summary para la respuesta
        result.DashboardSummary = await _dashboardService.GetDashboardAsync();

        return result;
    }

    public async Task<CreateExpenseResult> UpdateExpenseAsync(UpdateExpenseViewModel model)
    {
        var result = new CreateExpenseResult();

        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null)
        {
            result.ErrorMessage = "No se encontró la cuenta del usuario.";
            return result;
        }

        var expense = await _expenseRepository.GetByIdAsync(model.Id);
        if (expense == null)
        {
            result.ErrorMessage = "Gasto no encontrado.";
            return result;
        }

        var plan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(account.Id);
        if (plan == null || expense.MonthlyPlanId != plan.Id || plan.Status != PlanStatus.Active)
        {
            result.ErrorMessage = "El gasto no pertenece al plan activo o el plan está cerrado.";
            return result;
        }

        expense.Amount = model.Amount;
        expense.Category = model.Category;
        expense.Description = model.Description;

        await _expenseRepository.UpdateAsync(expense);
        await _expenseRepository.SaveChangesAsync();

        result.Succeeded = true;
        result.ExpenseId = expense.Id;
        result.Amount = expense.Amount;
        result.Category = expense.Category;
        result.CreatedAt = expense.CreatedAt;

        // Recalcular advertencias
        var expenses = await _expenseRepository.GetByMonthlyPlanIdAsync(plan.Id);
        var weekLimits = _clock.GetCurrentWeekLimitsUtc();
        var weeklyExpenses = await _expenseRepository.GetCurrentWeekByMonthlyPlanIdAsync(plan.Id, weekLimits.StartUtc, weekLimits.EndUtc);

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

        if (remainings.FreePocketRemaining < 0) result.Warnings.Add(ExpenseWarning.FreePocketExceeded);
        if (expense.Category == ExpenseCategory.Whim && remainings.WhimRemaining < 0) result.Warnings.Add(ExpenseWarning.WhimBudgetExceeded);
        if (expense.Category == ExpenseCategory.Life && remainings.LifeRemaining < 0) result.Warnings.Add(ExpenseWarning.LifeBudgetExceeded);
        if (remainings.WeeklyRemaining < 0) result.Warnings.Add(ExpenseWarning.WeeklyBudgetExceeded);

        result.DashboardSummary = await _dashboardService.GetDashboardAsync();

        return result;
    }

    public async Task<DeleteExpenseResult> DeleteExpenseAsync(Guid expenseId)
    {
        var result = new DeleteExpenseResult();

        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null)
        {
            result.ErrorMessage = "No se encontró la cuenta del usuario.";
            return result;
        }

        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense == null)
        {
            result.ErrorMessage = "Gasto no encontrado.";
            return result;
        }

        var plan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(account.Id);
        if (plan == null || expense.MonthlyPlanId != plan.Id || plan.Status != PlanStatus.Active)
        {
            result.ErrorMessage = "El gasto no pertenece al plan activo o el plan está cerrado.";
            return result;
        }

        await _expenseRepository.DeleteAsync(expense);
        await _expenseRepository.SaveChangesAsync();

        result.Succeeded = true;
        result.DashboardSummary = await _dashboardService.GetDashboardAsync();

        return result;
    }
}
