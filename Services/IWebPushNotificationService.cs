using PocketFlow.Models;

namespace PocketFlow.Services;

public enum WebPushResult
{
    Success,
    NoSubscriptions,
    AllInvalid,
    Failure
}

public interface IWebPushNotificationService
{
    Task<WebPushResult> SendNotificationAsync(Guid accountId, string title, string body, string url = "/", string tag = "default", string type = "generic");
    Task<WebPushResult> SendNotificationToEndpointAsync(Guid accountId, string endpoint, string title, string body, string url = "/", string tag = "default", string type = "generic");
    Task<WebPushResult> BroadcastNotificationAsync(string title, string body, string url = "/", string tag = "default", string type = "generic");
}
