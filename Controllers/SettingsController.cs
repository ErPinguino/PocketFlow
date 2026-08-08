using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFlow.Services;
using PocketFlow.Models;
using PocketFlow.Data;

namespace PocketFlow.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly IAccountContextService _accountContext;
        private readonly ApplicationDbContext _context;

        public SettingsController(IAccountContextService accountContext, ApplicationDbContext context)
        {
            _accountContext = accountContext;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var account = await _accountContext.GetCurrentAccountAsync();
            if (account == null) return RedirectToAction("Login", "Account");
            
            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateNotificationPreferences([FromBody] NotificationPreferencesViewModel model)
        {
            var account = await _accountContext.GetCurrentAccountAsync();
            if (account == null) return Unauthorized();

            account.NotifyPayday = model.NotifyPayday;
            account.NotifyWeeklyBudget = model.NotifyWeeklyBudget;
            account.NotifyPiggyBanks = model.NotifyPiggyBanks;
            account.NotifyExpenseReminders = model.NotifyExpenseReminders;

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }

    public class NotificationPreferencesViewModel
    {
        public bool NotifyPayday { get; set; }
        public bool NotifyWeeklyBudget { get; set; }
        public bool NotifyPiggyBanks { get; set; }
        public bool NotifyExpenseReminders { get; set; }
    }
}
