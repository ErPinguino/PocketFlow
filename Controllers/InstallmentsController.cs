using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFlow.Filters;
using PocketFlow.Services;
using PocketFlow.ViewModels.Installments;
using System.Linq;

namespace PocketFlow.Controllers;

[Authorize]
[RequireOnboarding]
[Route("[controller]")]
public class InstallmentsController : Controller
{
    private readonly IInstallmentService _installmentService;
    private readonly IAccountContextService _accountContextService;

    public InstallmentsController(IInstallmentService installmentService, IAccountContextService accountContextService)
    {
        _installmentService = installmentService;
        _accountContextService = accountContextService;
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateInstallmentPlanViewModel model, [FromServices] Microsoft.Extensions.Logging.ILogger<InstallmentsController> logger)
    {
        bool isJsonRequest = Request.Headers.Accept.ToString().Contains("application/json") ||
                             Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        try
        {
            var account = await _accountContextService.GetCurrentAccountAsync();
            if (account == null)
            {
                if (isJsonRequest) return BadRequest(new { succeeded = false, errorMessage = "No se ha podido identificar tu cuenta. Vuelve a iniciar sesión." });
                TempData["ErrorMessage"] = "No se ha podido identificar tu cuenta. Vuelve a iniciar sesión.";
                return RedirectToAction("Index", "Pocket");
            }

            if (!ModelState.IsValid)
            {
                if (isJsonRequest)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new { succeeded = false, errorMessage = string.Join(" ", errors) });
                }
                TempData["ErrorMessage"] = "Datos del plan inválidos.";
                return RedirectToAction("Index", "Pocket");
            }
            
            var result = await _installmentService.CreatePlanAsync(account.Id, model);
            
            if (isJsonRequest)
            {
                if (!result.Succeeded) return BadRequest(result);
                return Ok(result);
            }

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Pago a plazos registrado con éxito.";
                TempData["PlaySuccessSound"] = true;
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado al intentar crear un pago a plazos.");
            if (isJsonRequest) return BadRequest(new { succeeded = false, errorMessage = "Ocurrió un error inesperado al procesar tu solicitud." });
            TempData["ErrorMessage"] = "Ocurrió un error inesperado al procesar tu solicitud.";
        }
        
        return RedirectToAction("Index", "Pocket");
    }

    [HttpPost("{id:guid}/Liquidate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Liquidate(Guid id, [FromServices] Microsoft.Extensions.Logging.ILogger<InstallmentsController> logger)
    {
        bool isJsonRequest = Request.Headers.Accept.ToString().Contains("application/json") ||
                             Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        try
        {
            var account = await _accountContextService.GetCurrentAccountAsync();
            if (account == null)
            {
                if (isJsonRequest) return BadRequest(new { succeeded = false, errorMessage = "No se ha podido identificar tu cuenta. Vuelve a iniciar sesión." });
                TempData["ErrorMessage"] = "No se ha podido identificar tu cuenta. Vuelve a iniciar sesión.";
                return RedirectToAction("Index", "Pocket");
            }
            
            var result = await _installmentService.LiquidatePlanAsync(account.Id, id);
            
            if (isJsonRequest)
            {
                if (!result.Succeeded) return BadRequest(result);
                return Ok(result);
            }

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Pago a plazos liquidado con éxito.";
                TempData["PlaySuccessSound"] = true;
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado al intentar liquidar un pago a plazos con ID {PlanId}.", id);
            if (isJsonRequest) return BadRequest(new { succeeded = false, errorMessage = "Ocurrió un error inesperado al procesar tu solicitud." });
            TempData["ErrorMessage"] = "Ocurrió un error inesperado al procesar tu solicitud.";
        }
        
        return RedirectToAction("Index", "Pocket");
    }

    [HttpPost("{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, [FromServices] Microsoft.Extensions.Logging.ILogger<InstallmentsController> logger)
    {
        bool isJsonRequest = Request.Headers.Accept.ToString().Contains("application/json") ||
                             Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        try
        {
            var account = await _accountContextService.GetCurrentAccountAsync();
            if (account == null)
            {
                if (isJsonRequest) return BadRequest(new { succeeded = false, errorMessage = "No se ha podido identificar tu cuenta. Vuelve a iniciar sesión." });
                TempData["ErrorMessage"] = "No se ha podido identificar tu cuenta. Vuelve a iniciar sesión.";
                return RedirectToAction("Index", "Pocket");
            }
            
            var result = await _installmentService.DeletePlanAsync(account.Id, id);
            
            if (isJsonRequest)
            {
                if (!result.Succeeded) return BadRequest(result);
                return Ok(result);
            }

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Pago a plazos eliminado con éxito.";
                TempData["PlayDeleteSound"] = true;
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado al intentar eliminar un pago a plazos con ID {PlanId}.", id);
            if (isJsonRequest) return BadRequest(new { succeeded = false, errorMessage = "Ocurrió un error inesperado al procesar tu solicitud." });
            TempData["ErrorMessage"] = "Ocurrió un error inesperado al procesar tu solicitud.";
        }
        
        return RedirectToAction("Index", "Pocket");
    }

    [HttpPost("{id:guid}/Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, [FromServices] Microsoft.Extensions.Logging.ILogger<InstallmentsController> logger)
    {
        bool isJsonRequest = Request.Headers.Accept.ToString().Contains("application/json") ||
                             Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        try
        {
            var account = await _accountContextService.GetCurrentAccountAsync();
            if (account == null)
            {
                if (isJsonRequest) return BadRequest(new { succeeded = false, errorMessage = "No se ha podido identificar tu cuenta. Vuelve a iniciar sesión." });
                TempData["ErrorMessage"] = "No se ha podido identificar tu cuenta. Vuelve a iniciar sesión.";
                return RedirectToAction("Index", "Pocket");
            }
            
            var result = await _installmentService.CancelPlanAsync(account.Id, id);
            
            if (isJsonRequest)
            {
                if (!result.Succeeded) return BadRequest(result);
                return Ok(result);
            }

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Pago a plazos cancelado con éxito.";
                TempData["PlayDeleteSound"] = true;
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado al intentar cancelar un pago a plazos con ID {PlanId}.", id);
            if (isJsonRequest) return BadRequest(new { succeeded = false, errorMessage = "Ocurrió un error inesperado al procesar tu solicitud." });
            TempData["ErrorMessage"] = "Ocurrió un error inesperado al procesar tu solicitud.";
        }
        
        return RedirectToAction("Index", "Pocket");
    }
}
