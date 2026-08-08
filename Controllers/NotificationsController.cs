using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PocketFlow.Data;
using PocketFlow.Models;
using PocketFlow.Services;

namespace PocketFlow.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAccountContextService _accountContext;
    private readonly WebPushOptions _options;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        ApplicationDbContext context, 
        IAccountContextService accountContext, 
        IOptions<WebPushOptions> options, 
        ILogger<NotificationsController> logger)
    {
        _context = context;
        _accountContext = accountContext;
        _options = options.Value;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult PublicKey()
    {
        return Json(new { publicKey = _options.PublicKey });
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionViewModel model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Endpoint))
        {
            return BadRequest(new { success = false, message = "Invalid subscription payload." });
        }

        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null) return Unauthorized();
        var accountId = account.Id;

        var existingSub = await _context.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == model.Endpoint);

        if (existingSub != null)
        {
            // If the endpoint is already registered, just update the AccountId (if user changed) and auth keys
            existingSub.AccountId = accountId;
            existingSub.P256dh = model.P256dh;
            existingSub.Auth = model.Auth;
            existingSub.IsActive = true;
            existingSub.UserAgent = Request.Headers["User-Agent"].ToString();
            
            _context.PushSubscriptions.Update(existingSub);
        }
        else
        {
            var newSub = new PushSubscription
            {
                AccountId = accountId,
                Endpoint = model.Endpoint,
                P256dh = model.P256dh,
                Auth = model.Auth,
                IsActive = true,
                UserAgent = Request.Headers["User-Agent"].ToString()
            };
            
            _context.PushSubscriptions.Add(newSub);
        }

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Push subscription registered for Account {AccountId}", accountId);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Unsubscribe([FromBody] PushSubscriptionViewModel model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Endpoint))
        {
            return BadRequest(new { success = false, message = "Invalid subscription payload." });
        }
        
        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null) return Unauthorized();
        var accountId = account.Id;

        var existingSub = await _context.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == model.Endpoint && s.AccountId == accountId);

        if (existingSub != null)
        {
            _context.PushSubscriptions.Remove(existingSub);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Push subscription removed for Account {AccountId}", accountId);
        }

        return Json(new { success = true });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendTest([FromServices] IWebPushNotificationService pushService)
    {
        var account = await _accountContext.GetCurrentAccountAsync();
        if (account == null) return Unauthorized();

        var result = await pushService.SendNotificationAsync(
            accountId: account.Id,
            title: "PocketFlow",
            body: "Las notificaciones funcionan correctamente.",
            url: "/Dashboard",
            tag: "pocketflow-test"
        );

        return Json(new { result = result.ToString() });
    }
}

public class PushSubscriptionViewModel
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}
