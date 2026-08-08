namespace PocketFlow.Models;

public class PushSubscription : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    
    public string UserAgent { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}
