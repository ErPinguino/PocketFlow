using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PocketFlow.Data;
using PocketFlow.Models;
using WebPush;

namespace PocketFlow.Services;

public class WebPushNotificationService : IWebPushNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly WebPushOptions _options;
    private readonly WebPushClient _webPushClient;
    private readonly ILogger<WebPushNotificationService> _logger;

    public WebPushNotificationService(
        ApplicationDbContext context, 
        IOptions<WebPushOptions> options, 
        ILogger<WebPushNotificationService> logger)
    {
        _context = context;
        _options = options.Value;
        _logger = logger;
        _webPushClient = new WebPushClient();
    }

    public async Task<WebPushResult> SendNotificationAsync(Guid accountId, string title, string body, string url = "/", string tag = "default")
    {
        var subscriptions = await _context.PushSubscriptions
            .Where(s => s.AccountId == accountId && s.IsActive)
            .ToListAsync();

        if (!subscriptions.Any()) return WebPushResult.NoSubscriptions;

        var payload = JsonSerializer.Serialize(new
        {
            title,
            body,
            url,
            tag
        });

        return await SendToSubscriptionsAsync(subscriptions, payload);
    }

    public async Task<WebPushResult> BroadcastNotificationAsync(string title, string body, string url = "/", string tag = "default")
    {
        var subscriptions = await _context.PushSubscriptions
            .Where(s => s.IsActive)
            .ToListAsync();

        if (!subscriptions.Any()) return WebPushResult.NoSubscriptions;

        var payload = JsonSerializer.Serialize(new
        {
            title,
            body,
            url,
            tag
        });

        return await SendToSubscriptionsAsync(subscriptions, payload);
    }

    private async Task<WebPushResult> SendToSubscriptionsAsync(List<PocketFlow.Models.PushSubscription> subscriptions, string payload)
    {
        var vapidDetails = new VapidDetails(_options.Subject, _options.PublicKey, _options.PrivateKey);
        var subscriptionsToRemove = new List<PocketFlow.Models.PushSubscription>();
        int successCount = 0;
        int failureCount = 0;

        foreach (var sub in subscriptions)
        {
            try
            {
                var pushSubscription = new WebPush.PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                await _webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
                successCount++;
            }
            catch (WebPushException exception)
            {
                _logger.LogWarning(exception, "Failed to send push notification to endpoint {Endpoint}. StatusCode: {StatusCode}", sub.Endpoint, exception.StatusCode);
                
                // If it's 404 (Not Found) or 410 (Gone), the subscription has expired or is no longer valid.
                if (exception.StatusCode == System.Net.HttpStatusCode.Gone || exception.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    subscriptionsToRemove.Add(sub);
                }
                failureCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending push notification to endpoint {Endpoint}", sub.Endpoint);
                failureCount++;
            }
        }

        if (subscriptionsToRemove.Any())
        {
            _context.PushSubscriptions.RemoveRange(subscriptionsToRemove);
            await _context.SaveChangesAsync();
        }

        if (successCount > 0)
        {
            _logger.LogInformation("Push notification sent to {Count} active subscriptions.", successCount);
            return WebPushResult.Success;
        }

        if (subscriptionsToRemove.Count == subscriptions.Count)
        {
            return WebPushResult.AllInvalid;
        }

        return WebPushResult.Failure;
    }
}
