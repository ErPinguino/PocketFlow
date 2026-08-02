using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFlow.Services;
using PocketFlow.ViewModels.Onboarding;
using System.Security.Claims;

namespace PocketFlow.Controllers;

[Authorize]
public class OnboardingController : Controller
{
    private readonly IOnboardingStateService _stateService;
    private readonly IOnboardingService _onboardingService;
    private readonly IAuthenticationSessionService _authSessionService;

    public OnboardingController(
        IOnboardingStateService stateService,
        IOnboardingService onboardingService,
        IAuthenticationSessionService authSessionService)
    {
        _stateService = stateService;
        _onboardingService = onboardingService;
        _authSessionService = authSessionService;
    }

    private bool IsOnboardingCompleted()
    {
        return User.FindFirst("onboarding_completed")?.Value == "true";
    }

    private Guid GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(idClaim!);
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        return RedirectToAction("Welcome");
    }

    [HttpGet]
    public IActionResult Welcome()
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpGet]
    public IActionResult Account()
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        var state = _stateService.GetState();
        var model = new OnboardingAccountViewModel
        {
            AccountName = state.AccountName == string.Empty ? "Principal" : state.AccountName,
            Currency = string.IsNullOrEmpty(state.Currency) ? "EUR" : state.Currency,
            MonthlyIncome = state.MonthlyIncome,
            Payday = state.Payday == 0 ? 1 : state.Payday
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Account(OnboardingAccountViewModel model)
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        
        if (!ModelState.IsValid) return View(model);

        var state = _stateService.GetState();
        state.AccountName = model.AccountName;
        state.Currency = model.Currency;
        state.MonthlyIncome = model.MonthlyIncome;
        state.Payday = model.Payday;
        _stateService.SaveState(state);

        return RedirectToAction("FixedExpenses");
    }

    [HttpGet]
    public IActionResult FixedExpenses()
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        
        var state = _stateService.GetState();
        if (string.IsNullOrEmpty(state.AccountName)) return RedirectToAction("Account");

        var model = new OnboardingFixedExpensesViewModel
        {
            FixedExpenses = state.FixedExpenses
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FixedExpenses(OnboardingFixedExpensesViewModel model)
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        
        if (!ModelState.IsValid) return View(model);

        var state = _stateService.GetState();
        state.FixedExpenses = model.FixedExpenses;
        _stateService.SaveState(state);

        return RedirectToAction("PiggyBanks");
    }

    [HttpGet]
    public IActionResult PiggyBanks()
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        var state = _stateService.GetState();
        
        var model = new OnboardingPiggyBanksViewModel
        {
            PiggyBanks = state.PiggyBanks.Any() ? state.PiggyBanks : new List<OnboardingPiggyBankItemViewModel>
            {
                new OnboardingPiggyBankItemViewModel { Name = "Colchón de emergencia", Icon = "🛡️" }
            }
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PiggyBanks(OnboardingPiggyBanksViewModel model)
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        
        if (!ModelState.IsValid) return View(model);

        var state = _stateService.GetState();
        state.PiggyBanks = model.PiggyBanks ?? new List<OnboardingPiggyBankItemViewModel>();
        _stateService.SaveState(state);

        return RedirectToAction("Pocket");
    }

    [HttpGet]
    public IActionResult Pocket([FromServices] IFinancialCalculationService calcService)
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        
        var state = _stateService.GetState();
        if (string.IsNullOrEmpty(state.AccountName)) return RedirectToAction("Account");
        
        var totalSavings = calcService.CalculateTotalMonthlySavings(state.PiggyBanks.Select(p => p.MonthlyContribution));
        var availableFree = calcService.CalculateAvailableFreePocket(state.MonthlyIncome, state.FixedExpenses, totalSavings);

        var model = new OnboardingPocketViewModel
        {
            MonthlyIncome = state.MonthlyIncome,
            FixedExpenses = state.FixedExpenses,
            TotalMonthlySavings = totalSavings,
            AvailableFreePocket = availableFree,
            LifeBudget = state.LifeBudget == 0 && state.WhimBudget == 0 ? availableFree : state.LifeBudget,
            WhimBudget = state.WhimBudget
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Pocket(OnboardingPocketViewModel model, [FromServices] IFinancialCalculationService calcService)
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        
        var state = _stateService.GetState();
        var totalSavings = calcService.CalculateTotalMonthlySavings(state.PiggyBanks.Select(p => p.MonthlyContribution));
        var availableFree = calcService.CalculateAvailableFreePocket(state.MonthlyIncome, state.FixedExpenses, totalSavings);

        if (!calcService.ValidatePocketBudgets(availableFree, model.LifeBudget, model.WhimBudget))
        {
            ModelState.AddModelError(string.Empty, "La suma de Vida y Caprichos debe coincidir exactamente con el Bolsillo Libre.");
        }

        if (!ModelState.IsValid)
        {
            model.MonthlyIncome = state.MonthlyIncome;
            model.FixedExpenses = state.FixedExpenses;
            model.TotalMonthlySavings = totalSavings;
            model.AvailableFreePocket = availableFree;
            return View(model);
        }

        state.LifeBudget = model.LifeBudget;
        state.WhimBudget = model.WhimBudget;
        _stateService.SaveState(state);

        return RedirectToAction("Summary");
    }

    [HttpGet]
    public IActionResult Summary([FromServices] IFinancialCalculationService calcService)
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        
        var state = _stateService.GetState();
        if (string.IsNullOrEmpty(state.AccountName)) return RedirectToAction("Account");
        
        var totalSavings = calcService.CalculateTotalMonthlySavings(state.PiggyBanks.Select(p => p.MonthlyContribution));
        var availableFree = calcService.CalculateAvailableFreePocket(state.MonthlyIncome, state.FixedExpenses, totalSavings);
        var weekly = calcService.CalculateWeeklyBudget(availableFree);

        state.TotalMonthlySavings = totalSavings;
        state.AvailableFreePocket = availableFree;
        state.WeeklyBudget = weekly;
        
        return View(state);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete()
    {
        if (IsOnboardingCompleted()) return RedirectToAction("Index", "Dashboard");
        
        var state = _stateService.GetState();
        var userId = GetUserId();

        var success = await _onboardingService.CompleteOnboardingAsync(userId, state);
        if (success)
        {
            _stateService.ClearState();
            await _authSessionService.RenewOnboardingCompletedClaimAsync(User);
            return RedirectToAction("Index", "Dashboard");
        }

        TempData["ErrorMessage"] = "Hubo un problema al crear tu plan. Por favor, intenta de nuevo.";
        return RedirectToAction("Summary");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reset()
    {
        _stateService.ClearState();
        return RedirectToAction("Welcome");
    }
}
