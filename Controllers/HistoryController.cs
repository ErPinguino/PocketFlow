using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFlow.Filters;
using PocketFlow.Services;

namespace PocketFlow.Controllers;

[Authorize]
[RequireOnboarding]
public class HistoryController : Controller
{
    private readonly IMonthlyHistoryService _historyService;

    public HistoryController(IMonthlyHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _historyService.GetHistoryListAsync();
        
        if (model == null)
            return NotFound();

        return View(model);
    }

    [HttpGet]
    [Route("History/Detail/{planId:guid}")]
    public async Task<IActionResult> Detail(Guid planId)
    {
        var model = await _historyService.GetHistoricalDashboardAsync(planId);
        
        if (model == null)
            return NotFound(); // This handles security implicitly if they request someone else's plan.

        return View(model);
    }
}
