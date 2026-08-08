using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFlow.Filters;
using PocketFlow.Services;
using PocketFlow.ViewModels.Expenses;

namespace PocketFlow.Controllers;

[Authorize]
[RequireOnboarding]
public class ExpensesController : Controller
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateExpenseViewModel model)
    {
        bool isJsonRequest = Request.Headers.Accept.ToString().Contains("application/json") ||
                             Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!ModelState.IsValid)
        {
            if (isJsonRequest)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { succeeded = false, errorMessage = string.Join(" ", errors) });
            }
            
            // Fallback MVC
            TempData["ErrorMessage"] = "Por favor, revisa los datos del gasto.";
            return RedirectToAction("Index", "Dashboard");
        }

        var result = await _expenseService.CreateExpenseAsync(model);

        if (isJsonRequest)
        {
            return Ok(result);
        }

        // Fallback MVC
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Gasto registrado correctamente.";
            if (result.Warnings.Any())
            {
                TempData["WarningMessage"] = "Gasto guardado, pero se han superado algunos presupuestos.";
            }
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateExpenseViewModel model)
    {
        bool isJsonRequest = Request.Headers.Accept.ToString().Contains("application/json") ||
                             Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!ModelState.IsValid)
        {
            if (isJsonRequest)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { succeeded = false, errorMessage = string.Join(" ", errors) });
            }
            
            TempData["ErrorMessage"] = "Por favor, revisa los datos del gasto.";
            return RedirectToAction("Index", "Pocket");
        }

        var result = await _expenseService.UpdateExpenseAsync(model);

        if (isJsonRequest)
        {
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Gasto actualizado correctamente.";
        }

        return RedirectToAction("Index", "Pocket");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool isJsonRequest = Request.Headers.Accept.ToString().Contains("application/json") ||
                             Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        var result = await _expenseService.DeleteExpenseAsync(id);

        if (isJsonRequest)
        {
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Gasto eliminado correctamente.";
        }

        return RedirectToAction("Index", "Pocket");
    }
}
