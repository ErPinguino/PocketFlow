using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFlow.Filters;
using PocketFlow.Services;
using PocketFlow.ViewModels.PiggyBanks;
using System;
using System.Threading.Tasks;

namespace PocketFlow.Controllers;

[Authorize]
[RequireOnboarding]
public class PiggyBanksController : Controller
{
    private readonly IPiggyBankService _piggyBankService;

    public PiggyBanksController(IPiggyBankService piggyBankService)
    {
        _piggyBankService = piggyBankService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = await _piggyBankService.GetAllAsync();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePiggyBankViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _piggyBankService.CreateAsync(model);

        if (result.Succeeded)
        {
            return Ok();
        }

        return BadRequest(new { error = result.ErrorMessage });
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(Guid id)
    {
        var model = await _piggyBankService.GetForEditAsync(id);
        if (model == null)
        {
            return NotFound();
        }
        return Json(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdatePiggyBankViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _piggyBankService.UpdateAsync(model);

        if (result.Succeeded)
        {
            return Ok();
        }

        return BadRequest(new { error = result.ErrorMessage });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(Guid id)
    {
        var result = await _piggyBankService.ArchiveAsync(id);

        if (result.Succeeded)
        {
            return Ok();
        }

        return BadRequest(new { error = result.ErrorMessage });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(Guid id)
    {
        var result = await _piggyBankService.ReactivateAsync(id);

        if (result.Succeeded)
        {
            return Ok();
        }

        return BadRequest(new { error = result.ErrorMessage });
    }
}
