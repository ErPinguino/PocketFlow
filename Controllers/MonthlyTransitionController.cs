using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFlow.Models;
using PocketFlow.Repositories;
using PocketFlow.Services;
using PocketFlow.ViewModels.MonthlyTransition;

namespace PocketFlow.Controllers;

[Authorize]
public class MonthlyTransitionController : Controller
{
    private readonly IMonthlyTransitionService _transitionService;
    private readonly IAccountContextService _accountContext;
    private readonly IPiggyBankRepository _piggyBankRepository;
    private readonly IMonthlyPlanRepository _monthlyPlanRepository;
    private readonly IAppClock _clock;

    public MonthlyTransitionController(
        IMonthlyTransitionService transitionService,
        IAccountContextService accountContext,
        IPiggyBankRepository piggyBankRepository,
        IMonthlyPlanRepository monthlyPlanRepository,
        IAppClock clock)
    {
        _transitionService = transitionService;
        _accountContext = accountContext;
        _piggyBankRepository = piggyBankRepository;
        _monthlyPlanRepository = monthlyPlanRepository;
        _clock = clock;
    }

    [HttpGet]
    public async Task<IActionResult> Start()
    {
        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null) return RedirectToAction("Login", "Account");

        var leftover = await _transitionService.CalculateLeftoverAsync(account.Id);
        
        if (leftover > 0)
        {
            var pbs = await _piggyBankRepository.GetActiveByAccountIdAsync(account.Id);
            var vm = new RolloverDecisionViewModel
            {
                LeftoverAmount = leftover,
                AvailablePiggyBanks = pbs.Select(p => new PiggyBankSelectionItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    Icon = p.Icon
                }).ToList()
            };
            return View("Step1_Rollover", vm);
        }

        // Si no hay leftover, saltar directamente a preguntar sobre el plan
        TempData["RolloverDecision"] = JsonSerializer.Serialize(new RolloverDecisionViewModel
        {
            LeftoverAmount = leftover,
            DestinationType = RolloverDestinationType.Unassigned
        });

        return RedirectToAction("Step2_PlanPrompt");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Step1_Rollover(RolloverDecisionViewModel model)
    {
        if (model.DestinationType == RolloverDestinationType.PiggyBank && !model.DestinationPiggyBankId.HasValue)
        {
            ModelState.AddModelError("DestinationPiggyBankId", "Debes seleccionar una hucha.");
        }

        if (!ModelState.IsValid)
        {
            return RedirectToAction("Start"); // For MVP: simple fallback
        }

        TempData["RolloverDecision"] = JsonSerializer.Serialize(model);
        return RedirectToAction("Step2_PlanPrompt");
    }

    [HttpGet]
    public IActionResult Step2_PlanPrompt()
    {
        return View("Step2_PlanPrompt");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Step2_PlanPrompt(string decision)
    {
        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null) return RedirectToAction("Login", "Account");

        var localNow = _clock.LocalNow;
        var activePlan = await _monthlyPlanRepository.GetActivePlanByAccountIdAsync(account.Id);
        var pbs = await _piggyBankRepository.GetActiveByAccountIdAsync(account.Id);

        var vm = new MonthlyPlanReviewViewModel
        {
            MonthlyIncome = activePlan?.Income ?? account.MonthlyIncome,
            FixedExpenses = activePlan?.FixedExpenses ?? 0,
            LifeBudget = activePlan?.LifeBudget ?? 0,
            WhimBudget = activePlan?.WhimBudget ?? 0,
            PiggyBanks = pbs.Select(p => new PiggyBankReviewItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Icon = p.Icon,
                IsActive = p.IsActive,
                MonthlyContribution = p.MonthlyContribution
            }).ToList()
        };

        if (decision == "keep")
        {
            TempData["PlanReview"] = JsonSerializer.Serialize(vm);
            return RedirectToAction("Step4_Summary");
        }

        return View("Step3_PlanReview", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Step3_PlanReview(MonthlyPlanReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["PlanReview"] = JsonSerializer.Serialize(model);
        return RedirectToAction("Step4_Summary");
    }

    [HttpGet]
    public async Task<IActionResult> Step4_Summary()
    {
        var rolloverJson = TempData.Peek("RolloverDecision") as string;
        var reviewJson = TempData.Peek("PlanReview") as string;

        if (string.IsNullOrEmpty(rolloverJson) || string.IsNullOrEmpty(reviewJson))
        {
            return RedirectToAction("Start");
        }

        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null) return RedirectToAction("Login", "Account");

        var rolloverDecision = JsonSerializer.Deserialize<RolloverDecisionViewModel>(rolloverJson);
        var planReview = JsonSerializer.Deserialize<MonthlyPlanReviewViewModel>(reviewJson);

        if (rolloverDecision == null || planReview == null)
        {
            return RedirectToAction("Start");
        }

        var summary = await _transitionService.PrepareTransitionSummaryAsync(account.Id, rolloverDecision, planReview);
        return View("Step4_Summary", summary);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Step4_Summary_Confirm()
    {
        var rolloverJson = TempData["RolloverDecision"] as string;
        var reviewJson = TempData["PlanReview"] as string;

        if (string.IsNullOrEmpty(rolloverJson) || string.IsNullOrEmpty(reviewJson))
        {
            return RedirectToAction("Start");
        }

        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null) return RedirectToAction("Login", "Account");

        var rolloverDecision = JsonSerializer.Deserialize<RolloverDecisionViewModel>(rolloverJson);
        var planReview = JsonSerializer.Deserialize<MonthlyPlanReviewViewModel>(reviewJson);

        if (rolloverDecision == null || planReview == null)
        {
            return RedirectToAction("Start");
        }

        var success = await _transitionService.ExecuteTransitionAsync(account.Id, rolloverDecision, planReview);
        
        if (success)
        {
            TempData["SuccessMessage"] = "Tu nuevo mes financiero está listo.";
            return RedirectToAction("Index", "Dashboard");
        }

        TempData["ErrorMessage"] = "No se pudo crear el nuevo mes. Puede que ya haya sido creado o hubo un problema técnico.";
        return RedirectToAction("Index", "Dashboard");
    }
}
