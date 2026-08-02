using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFlow.Filters;
using PocketFlow.Services;

namespace PocketFlow.Controllers;

[Authorize]
[RequireOnboarding]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _dashboardService.GetDashboardAsync();

        if (model == null)
        {
            return View("NoData");
        }

        return View(model);
    }
}
