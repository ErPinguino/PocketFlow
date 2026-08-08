using PocketFlow.Models;

namespace PocketFlow.Services;

public interface IWebPushNotificationService
{
    Task SendNotificationAsync(Guid accountId, string title, string body, string url = "/", string tag = "default");
    Task BroadcastNotificationAsync(string title, string body, string url = "/", string tag = "default");
}
