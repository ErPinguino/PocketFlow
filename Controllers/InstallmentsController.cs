using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFlow.Services;
using PocketFlow.ViewModels.Installments;

namespace PocketFlow.Controllers;

[Authorize]
[Route("[controller]")]
public class InstallmentsController : Controller
{
    private readonly IInstallmentService _installmentService;

    public InstallmentsController(IInstallmentService installmentService)
    {
        _installmentService = installmentService;
    }

    private Guid GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(idClaim!);
    }

    private Guid GetAccountId()
    {
        var accountIdClaim = User.FindFirst("AccountId")?.Value;
        return Guid.Parse(accountIdClaim!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateInstallmentPlanViewModel model)
    {
        var accountId = GetAccountId();
        
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Datos del plan inválidos.";
            return RedirectToAction("Index", "Pocket");
        }
        
        var result = await _installmentService.CreatePlanAsync(accountId, model);
        
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Pago a plazos registrado con éxito.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        
        return RedirectToAction("Index", "Pocket");
    }

    [HttpPost("{id:guid}/Liquidate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Liquidate(Guid id)
    {
        var accountId = GetAccountId();
        
        var result = await _installmentService.LiquidatePlanAsync(accountId, id);
        
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Pago a plazos liquidado con éxito.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        
        return RedirectToAction("Index", "Pocket");
    }
}
