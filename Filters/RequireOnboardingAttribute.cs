using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PocketFlow.Filters;

public class RequireOnboardingAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var onboardingClaim = user.FindFirst("onboarding_completed")?.Value;
            if (onboardingClaim != "true")
            {
                context.Result = new RedirectToActionResult("Index", "Onboarding", null);
            }
        }
        
        base.OnActionExecuting(context);
    }
}
