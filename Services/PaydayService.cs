using PocketFlow.Models;

namespace PocketFlow.Services;

public class PaydayService : IPaydayService
{
    private readonly IAppClock _clock;

    public PaydayService(IAppClock clock)
    {
        _clock = clock;
    }

    public bool ShouldAskPaydayConfirmation(Account account)
    {
        var localNow = _clock.LocalNow;
        
        var mostRecentPayday = GetMostRecentPayday(localNow, account.Payday);
        
        if (account.LastPaycheckConfirmedAt.HasValue)
        {
            var lastConfirmedLocal = TimeZoneInfo.ConvertTimeFromUtc(account.LastPaycheckConfirmedAt.Value, _clock.LocalTimeZone);
            // Si la última vez que confirmó fue en o después de nuestro último Payday, entonces ya está al día.
            if (lastConfirmedLocal.Date >= mostRecentPayday.Date)
            {
                return false;
            }
        }
        
        // Si no tiene fecha (raro porque ahora lo configuramos en el onboarding, pero por si acaso) o es anterior al Payday
        return true;
    }

    private DateTime GetMostRecentPayday(DateTime localNow, int payday)
    {
        int maxDaysInMonth = DateTime.DaysInMonth(localNow.Year, localNow.Month);
        int actualPaydayDay = payday > maxDaysInMonth ? maxDaysInMonth : payday;
        var paydayThisMonth = new DateTime(localNow.Year, localNow.Month, actualPaydayDay);
        
        if (localNow.Date >= paydayThisMonth.Date)
        {
            return paydayThisMonth;
        }
        
        var lastMonth = localNow.AddMonths(-1);
        int maxDaysLastMonth = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);
        int actualPaydayLastMonth = payday > maxDaysLastMonth ? maxDaysLastMonth : payday;
        return new DateTime(lastMonth.Year, lastMonth.Month, actualPaydayLastMonth);
    }
}
