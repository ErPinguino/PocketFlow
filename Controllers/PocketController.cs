using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFlow.Filters;
using PocketFlow.Helpers;
using PocketFlow.Models;
using PocketFlow.Repositories;
using PocketFlow.Services;
using PocketFlow.ViewModels.Pocket;

namespace PocketFlow.Controllers;

[Authorize]
[RequireOnboarding]
public class PocketController : Controller
{
    private readonly IAccountContextService _accountContext;
    private readonly IMonthlyPlanRepository _monthlyPlanRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IAppClock _clock;
    private readonly ILogger<PocketController> _logger;

    public PocketController(
        IAccountContextService accountContext,
        IMonthlyPlanRepository monthlyPlanRepository,
        IExpenseRepository expenseRepository,
        IAppClock clock,
        ILogger<PocketController> logger)
    {
        _accountContext = accountContext;
        _monthlyPlanRepository = monthlyPlanRepository;
        _expenseRepository = expenseRepository;
        _clock = clock;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromServices] IInstallmentMaterializationService installmentMaterializationService,
        ExpenseCategory? category = null, int page = 1, string? search = null, string? sort = null)
    {
        const int pageSize = 20;

        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null) return RedirectToAction("Login", "Account");
        
        await installmentMaterializationService.MaterializePendingInstallmentsAsync(account.Id);

        var localNow = _clock.LocalNow;
        var plan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(account.Id);

        if (plan == null)
        {
            return View("NoData");
        }

        var (items, totalCount) = await _expenseRepository.GetPagedByMonthlyPlanIdAsync(plan.Id, page, pageSize, category, search, sort);

        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
        if (page > totalPages && totalPages > 0)
        {
            page = totalPages;
            var newPagedResult = await _expenseRepository.GetPagedByMonthlyPlanIdAsync(plan.Id, page, pageSize, category, search, sort);
            items = newPagedResult.Items;
        }

        var allExpenses = await _expenseRepository.GetByMonthlyPlanIdAsync(plan.Id);
        var weekLimits = _clock.GetCurrentWeekLimitsUtc();
        var weeklyExpenses = await _expenseRepository.GetCurrentWeekByMonthlyPlanIdAsync(plan.Id, weekLimits.StartUtc, weekLimits.EndUtc);

        var freePocketSpent = allExpenses.Sum(e => e.Amount);
        var lifeSpent = allExpenses.Where(e => e.Category == ExpenseCategory.Life).Sum(e => e.Amount);
        var whimSpent = allExpenses.Where(e => e.Category == ExpenseCategory.Whim).Sum(e => e.Amount);
        var weeklySpent = weeklyExpenses.Sum(e => e.Amount);

        var calcService = HttpContext.RequestServices.GetRequiredService<IFinancialCalculationService>();
        var remainings = calcService.CalculatePlanRemainings(
            plan,
            freePocketSpent,
            lifeSpent,
            whimSpent,
            weeklySpent
        );

        var localZone = _clock.LocalTimeZone;

        ViewData["Search"] = search;
        ViewData["Sort"] = sort;

        var vm = new PocketViewModel
        {
            AccountId = account.Id,
            AccountName = account.Name,
            Currency = account.Currency,
            
            FreePocketRemaining = remainings.FreePocketRemaining,
            LifeRemaining = remainings.LifeRemaining,
            WhimRemaining = remainings.WhimRemaining,
            WeeklyRemaining = remainings.WeeklyRemaining,
            
            ActiveFilter = category,
            IsActivePlan = plan.Status == PlanStatus.Active,
            CurrentPage = page,
            TotalPages = totalPages,
            
            Expenses = items.Select(e => new ExpenseListItemViewModel
            {
                Id = e.Id,
                Amount = e.Amount,
                Category = e.Category,
                CategoryDisplayName = e.Category == ExpenseCategory.Life ? "Vida" : "Capricho",
                Description = e.Description,
                CreatedAtLocal = TimeZoneInfo.ConvertTimeFromUtc(e.CreatedAt, localZone),
                DateDisplay = e.CreatedAt.ToRelativeLocalString(localZone),
                IconClass = e.Category == ExpenseCategory.Life ? "bi-cart3 text-primary" : "bi-controller text-warning"
            }).ToList()
        };

        if (vm.TotalPages == 0) vm.TotalPages = 1;

        var installmentService = HttpContext.RequestServices.GetRequiredService<IInstallmentService>();
        vm.Installments = await installmentService.GetActivePlansAsync(account.Id);

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExpenseListPartial(Guid? planId = null, ExpenseCategory? category = null, int page = 1, string? search = null, string? sort = null)
    {
        const int pageSize = 20;

        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null) return Unauthorized();

        MonthlyPlan? plan;
        if (planId.HasValue)
        {
            plan = await _monthlyPlanRepository.GetByIdAndAccountIdAsync(planId.Value, account.Id);
        }
        else
        {
            plan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(account.Id);
        }

        if (plan == null) return NoContent();

        var (items, totalCount) = await _expenseRepository.GetPagedByMonthlyPlanIdAsync(plan.Id, page, pageSize, category, search, sort);

        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
        if (page > totalPages && totalPages > 0)
        {
            page = totalPages;
            var newPagedResult = await _expenseRepository.GetPagedByMonthlyPlanIdAsync(plan.Id, page, pageSize, category, search, sort);
            items = newPagedResult.Items;
        }

        var localZone = _clock.LocalTimeZone;

        var vm = new PocketViewModel
        {
            ActiveFilter = category,
            IsActivePlan = plan.Status == PlanStatus.Active,
            CurrentPage = page,
            TotalPages = totalPages,
            
            Expenses = items.Select(e => new ExpenseListItemViewModel
            {
                Id = e.Id,
                Amount = e.Amount,
                Category = e.Category,
                CategoryDisplayName = e.Category == ExpenseCategory.Life ? "Vida" : "Capricho",
                Description = e.Description,
                CreatedAtLocal = TimeZoneInfo.ConvertTimeFromUtc(e.CreatedAt, localZone),
                DateDisplay = e.CreatedAt.ToRelativeLocalString(localZone),
                IconClass = e.Category == ExpenseCategory.Life ? "bi-cart3 text-primary" : "bi-controller text-warning"
            }).ToList()
        };

        if (vm.TotalPages == 0) vm.TotalPages = 1;
        ViewData["Search"] = search;
        ViewData["Sort"] = sort;

        return PartialView("_ExpenseList", vm);
    }
}
