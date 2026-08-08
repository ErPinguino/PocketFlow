namespace PocketFlow.Models;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? SupabaseUserId { get; set; }
    public bool OnboardingCompleted { get; set; }
    
    public Account? Account { get; set; }
}
