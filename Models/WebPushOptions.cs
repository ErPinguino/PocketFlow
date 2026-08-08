namespace PocketFlow.Models;

public class WebPushOptions
{
    public const string WebPush = "WebPush";
    
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}
