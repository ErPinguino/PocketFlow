using System.Text.Json;
using PocketFlow.ViewModels.Onboarding;

namespace PocketFlow.Services;

public interface IOnboardingStateService
{
    OnboardingSummaryViewModel GetState();
    void SaveState(OnboardingSummaryViewModel state);
    void ClearState();
}

public class OnboardingStateService : IOnboardingStateService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string SessionKey = "OnboardingState";

    public OnboardingStateService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext?.Session 
                                ?? throw new InvalidOperationException("Session no disponible.");

    public OnboardingSummaryViewModel GetState()
    {
        var json = Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json))
        {
            return new OnboardingSummaryViewModel();
        }
        return JsonSerializer.Deserialize<OnboardingSummaryViewModel>(json) ?? new OnboardingSummaryViewModel();
    }

    public void SaveState(OnboardingSummaryViewModel state)
    {
        var json = JsonSerializer.Serialize(state);
        Session.SetString(SessionKey, json);
    }

    public void ClearState()
    {
        Session.Remove(SessionKey);
    }
}
