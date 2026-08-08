namespace PocketFlow.DTOs.Auth;

public class AuthResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    
    public Guid? UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public bool? OnboardingCompleted { get; set; }

    public static AuthResult Success(Guid userId, string name, string email, bool onboardingCompleted, string? avatarUrl = null) 
        => new() { Succeeded = true, UserId = userId, Name = name, Email = email, OnboardingCompleted = onboardingCompleted, AvatarUrl = avatarUrl };
        
    public static AuthResult Fail(string message) 
        => new() { Succeeded = false, ErrorMessage = message };
}
